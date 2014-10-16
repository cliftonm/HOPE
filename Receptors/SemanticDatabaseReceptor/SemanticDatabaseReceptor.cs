// #define SQLITE
#define POSTGRES

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Clifton.Assertions;
using Clifton.ExtensionMethods;
using Clifton.MycroParser;
using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;
using Clifton.Tools.Data;
using Clifton.Tools.Strings.Extensions;

// SQLite:
// list fields in a table: pragma table_info('sqlite_master')

namespace SemanticDatabaseReceptor
{
	public enum FieldValueType
	{
		NativeType,
		SemanticType,
	}

	public class FKValue
	{
		public string FieldName { get; protected set; }
		public int ID { get; protected set; }
		public bool UniqueField { get; protected set; }

		public FKValue(string fieldName, int id, bool uniqueField)
		{
			FieldName = fieldName;
			ID = id;
			UniqueField = uniqueField;
		}
	}

	public class FieldValue
	{
		public string FieldName { get; set; }
		public object Value { get; set; }
		public bool UniqueField { get; set; }
		public FieldValueType Type { get; set; }
		public TableFieldValues Parent { get; set; }

		public FieldValue(TableFieldValues parent, string fieldName, object val, bool unique, FieldValueType type)
		{
			Parent = parent;
			FieldName = fieldName;
			Value = val;
			UniqueField = unique;
			Type = type;
		}
	}

	public class TableFieldValues
	{
		public string TableName { get; set; }
		public bool UniqueField { get; set; }
		public List<FieldValue> FieldValues { get; protected set; }
		public int RecordID { get; set; }

		public TableFieldValues(string tableName, bool unique)
		{
			TableName = tableName;
			UniqueField = unique;
			FieldValues = new List<FieldValue>();
		}
	}

	public struct TypeIntersection
	{
		public string BaseType;
		public string JoinType;

		public TypeIntersection(string baseType, string joinType)
		{
			BaseType = baseType;
			JoinType = joinType;
		}
	}

	public class SemanticDatabase : BaseReceptor, ISemanticDatabase
	{
		public string DatabaseName { get; set; }
		public string DatabaseFileName { get { return DatabaseName + ".db"; } }
		public string PostgresConnectString { get { return "Server=127.0.0.1; Port=5432; User Id=" + postgresUserId + "; Password=" + postgresPassword + "; Database=" + DatabaseName; } }

		public override string Name { get { return "Semantic Database"; } }
		public override bool IsEdgeReceptor { get { return true; } }
		public override string ConfigurationUI { get { return "SemanticDatabaseConfig.xml"; } }

		private string postgresUserId;
		private string postgresPassword;

		/// <summary>
		/// For serialization only, not displayed on the configuration form.
		/// </summary>
		[UserConfigurableProperty("Internal")]
		public string Protocols { get; set; }

		/// <summary>
		/// Used for unit testing.
		/// </summary>
		public IDbConnection Connection { get { return dbio.Connection; } }
		public IDatabaseIO dbio;

		/// <summary>
		/// Determines whether carries are created always (test mode) or only if receiver exists (normal mode).
		/// </summary>
		public bool UnitTesting { get; set; }

		protected string connectionString;
		protected DataTable dt;
		protected DataGridView dgvTypes;

		public SemanticDatabase(IReceptorSystem rsys)
			: base(rsys)
		{
			DatabaseName = "hope_semantic_database";

			string[] postgresConfig = File.ReadAllLines("postgres.config");
			postgresUserId = postgresConfig[0];
			postgresPassword = postgresConfig[1];

			AddReceiveProtocol("Query", (Action<dynamic>)(signal => QueryDatabase((string)signal.QueryText)));

			// Test is made for the benefit of unit testing, which doesn't necessarily instantiate this message.
			if (rsys.SemanticTypeSystem.VerifyProtocolExists("LoggerMessage"))
			{
				AddEmitProtocol("LoggerMessage");
			}

			// Test is made for the benefit of unit testing, which doesn't necessarily instantiate this message.
			if (rsys.SemanticTypeSystem.VerifyProtocolExists("ExceptionMessage"))
			{
				AddEmitProtocol("ExceptionMessage");
			}
		}

		/// <summary>
		/// Support for unit testing.
		/// </summary>
		public void ProtocolsUpdated()
		{
			try
			{
				ValidateDatabaseSchema();
				UpdateListeners();
			}
			catch (Exception ex)
			{
				EmitException(ex);
			}
		}

		public override void EndSystemInit()
		{
			base.EndSystemInit();

			try
			{
				Connect();
				ValidateDatabaseSchema();
				UpdateListeners();
			}
			catch (Exception ex)
			{
				EmitException(ex);
			}
		}

		public override void Terminate()
		{
			try
			{
				dbio.Close();
			}
			catch
			{
				// On terminate, we don't really care about an exception, do we?
			}
		}

		public override void PrepopulateConfig(MycroParser mp)
		{
			base.PrepopulateConfig(mp);
			dgvTypes = (DataGridView)mp.ObjectCollection["dgvSemanticTypes"];

			if (dt == null)
			{
				// Create the table if it doesn't exist, otherwise
				// we populate the grid with the existing configuration.
				InitializeConfigTable();
			}

			dgvTypes.DataSource = new DataView(dt);

			// Stupid DGV doesn't pay attention to column captions:
			foreach (DataGridViewColumn col in dgvTypes.Columns)
			{
				col.HeaderText = dt.Columns[col.HeaderText].Caption;
			}
		}

		/// <summary>
		/// Update the received protocols given the new configuration.
		/// </summary>
		public override bool UserConfigurationUpdated()
		{
			bool ret = true;
			List<string> badProtocols = new List<string>();

			// TODO: Most of this (verifying a list of protocols) is probaby a rather common thing to do.  Move into STS as "VerifyProtocolsExist".

			(from row in dt.AsEnumerable()
			 where ((row[0] != null) && (!String.IsNullOrEmpty(row[0].ToString())))
			 select row[0].ToString()).ForEach(p =>
			 {
				 bool exists = rsys.SemanticTypeSystem.VerifyProtocolExists(p);

				 if (!exists)
				 {
					 badProtocols.Add(p);
					 ret = false;
				 }
			 });

			if (ret)
			{
				UpdateProtocolProperty();
				ValidateDatabaseSchema();
				UpdateListeners();
			}
			else
			{
				ConfigurationError = "The semantic type(s):\r\n" + String.Join("\r\n", badProtocols) + "\r\n do not exist.";
			}

			return ret;
		}

		// Process a carrier that we want to persist.
		public override void ProcessCarrier(ICarrier carrier)
		{
			base.ProcessCarrier(carrier);

			// TODO: This is a major kludge.  What we need to do is change Action to a Func, returning true of the base handler processed the carrier.
			if (carrier.Protocol.DeclTypeName == "Query")
			{
				return;
			}

			string st = carrier.Protocol.DeclTypeName;
			// List<TableFieldValues> tfvList = CreateTableFieldValueList(st, carrier.Signal);
			// List<TableFieldValues> tfvUniqueFieldList = tfvList.Where(t => t.UniqueField || t.FieldValues.Any(fv=>fv.UniqueField && fv.Type==FieldValueType.NativeType)).ToList();

			// Get the STS for the carrier's protocol:
			ISemanticTypeStruct sts = rsys.SemanticTypeSystem.GetSemanticTypeStruct(st);
			Dictionary<ISemanticTypeStruct, List<FKValue>> stfkMap = new Dictionary<ISemanticTypeStruct, List<FKValue>>();

			try
			{
				ProcessSTS(stfkMap, sts, carrier.Signal);
			}
			catch (Exception ex)
			{
				EmitException(ex);
			}
		}

		public void Connect()
		{
#if SQLITE
			// SQLite connection string
			connectionString = "Data Source = " + DatabaseFileName;
			dbio = new SQLiteIO();
			dbio.CreateDBIfMissing(DatabaseFileName);
			dbio.OpenDB(connectionString);
#endif

#if POSTGRES
			// Postgres connection string
			connectionString = PostgresConnectString;
			dbio = new PostgresIO();
			dbio.OpenDB(connectionString);
#endif
		}

		/// <summary>
		/// Updates the serializable UI property.
		/// </summary>
		protected void UpdateProtocolProperty()
		{
			StringBuilder sb = new StringBuilder();
			string and = String.Empty;

			foreach (DataRow row in dt.Rows)
			{
				string protocol = row[0].ToString();

				if (!String.IsNullOrEmpty(protocol))
				{
					// We use ',' and ';' internally, so strip those out.  Sorry user!
					protocol = protocol.Replace(",", "").Replace(";", "");

					// the format is [protocolname];[protocolname];...etc...
					sb.Append(and);
					sb.Append(protocol);
					and = ";";
				}
			}

			Protocols = sb.ToString();
		}

		/// <summary>
		/// Initializes the table used for configuration with the serialized tab-protocol list.
		/// </summary>
		protected void InitializeConfigTable()
		{
			dt = InitializeDataTable();

			if (!String.IsNullOrEmpty(Protocols))
			{
				string[] tpArray = Protocols.Split(';');

				foreach (string tp in tpArray)
				{
					DataRow row = dt.NewRow();
					row[0] = tp.Trim();
					dt.Rows.Add(row);
				}
			}
		}

		protected DataTable InitializeDataTable()
		{
			DataTable dt = new DataTable();
			DataColumn dcProtocol = new DataColumn("protocol");
			dcProtocol.Caption = "Protocol";
			dt.Columns.AddRange(new DataColumn[] { dcProtocol });

			return dt;
		}

		/// <summary>
		/// Verify that the tables and fields in the database match the semantic type structures.
		/// </summary>
		protected void ValidateDatabaseSchema()
		{
			if (!String.IsNullOrEmpty(Protocols))
			{
				List<string> tableNames = dbio.GetTables(this).Select(tbl => tbl.ToLower()).ToList();

				// TODO: Split by comma (which we seem to use everywhere else) or semicolon?
				// Maybe support both by figuring out which is used???
				string[] expectedRootTables = Protocols.Split(';');

				foreach (string expectedRootTable in expectedRootTables)
				{
					CreateIfMissing(expectedRootTable.Trim(), tableNames);
				}
			}
		}

		/// <summary>
		/// Inspects the table list for the specified table.  If not found, it creates the table.
		/// If found, the fields are inspected.
		/// Regardless, the sub-types are recursively inspected as well.
		/// </summary>
		protected void CreateIfMissing(string st, List<string> tableNames)
		{
			// All of these "ToLower's" is because Postgres converts table and field names to lowercase.  
			// Why in the world would it do that???  (Of course, if it's a quoted table or field name, then case is preserved!!!)
			if (!tableNames.Contains(st.ToLower()))
			{
				CreateTable(st);
				tableNames.Add(st.ToLower());
			}
			else
			{
				VerifyColumns(st.ToLower());
			}

			ISemanticTypeStruct sts = rsys.SemanticTypeSystem.GetSemanticTypeStruct(st);
			sts.SemanticElements.ForEach(child => CreateIfMissing(child.Name, tableNames));
		}

		protected void CreateTable(string st)
		{
			List<Tuple<string, Type>> fieldTypes = new List<Tuple<string, Type>>();
			ISemanticTypeStruct sts = rsys.SemanticTypeSystem.GetSemanticTypeStruct(st);

			// Create FK's for child SE's.
			sts.SemanticElements.ForEach(child =>
			{
				fieldTypes.Add(new Tuple<string, Type>("FK_" + child.Name + "ID", typeof(long)));
			});

			// Create fields for NT's.
			sts.NativeTypes.ForEach(child =>
				{
					Type t = child.GetImplementingType(rsys.SemanticTypeSystem);

					if (t != null)
					{
						fieldTypes.Add(new Tuple<string, Type>(child.Name, t));
					}
					else
					{
						// TODO: The reason for the try-catch is to deal with implementing types we don't support yet, like List<dynamic>
						// For now, we create a stub type.
						fieldTypes.Add(new Tuple<string, Type>(child.Name, typeof(string)));
					}
				});

			dbio.CreateTable(this, st, fieldTypes);
		}

		protected void VerifyColumns(string st)
		{
			List<string> colNames = dbio.GetColumns(this, st);
			// TODO: Finish implementation.
		}

		protected void UpdateListeners()
		{
			if (!(String.IsNullOrEmpty(Protocols)))
			{
				// TODO: Remove protocols that are not being listened to anymore
				Protocols.Split(';').ForEach(p => AddReceiveProtocol(p.Trim()));
			}
		}

		protected List<TableFieldValues> CreateTableFieldValueList(string st, dynamic signal)
		{
			List<TableFieldValues> tfvList = new List<TableFieldValues>();
			CreateTableFieldValueList(tfvList, st, signal);

			return tfvList;
		}

		/// <summary>
		/// Recursive call to get table field values the specified semantic type.
		/// </summary>
		protected void CreateTableFieldValueList(List<TableFieldValues> tfvList, string st, dynamic signal)
		{
			ISemanticTypeStruct sts = rsys.SemanticTypeSystem.GetSemanticTypeStruct(st);
			TableFieldValues tfvEntry = new TableFieldValues(st, sts.Unique);
			tfvList.Add(tfvEntry);

			sts.SemanticElements.ForEach(child =>
			{
				string fieldName = "FK_" + child.Name + "ID";
				tfvEntry.FieldValues.Add(new FieldValue(tfvEntry, fieldName, null, child.Element.Struct.Unique, FieldValueType.SemanticType));
				PropertyInfo piSub = signal.GetType().GetProperty(child.Name);
				object childSignal = piSub.GetValue(signal);
				CreateTableFieldValueList(tfvList, child.Name, childSignal);
			});

			foreach (INativeType nt in sts.NativeTypes)
			{
				// Acquire value through reflection.
				object val = nt.GetValue(rsys.SemanticTypeSystem, signal);
				tfvEntry.FieldValues.Add(new FieldValue(tfvEntry, nt.Name, val, nt.UniqueField, FieldValueType.NativeType));
			}
		}

		protected int ProcessSTS(Dictionary<ISemanticTypeStruct, List<FKValue>> stfkMap, ISemanticTypeStruct sts, object signal, bool childAsUnique = false)
		{
			int id = -1;

			// Is this ST a bottom-most ST, such that it only implements native types?
			if (sts.HasSemanticTypes)
			{
				// Nope, we're somewehere higher up.
				// Drill into each child ST and assign the return ID to this ST's FK for the child table name.
				sts.SemanticElements.ForEach(child =>
					{
						// Get the child signal and STS and check it, returning a new or existing ID for the entry.
						ISemanticTypeStruct childsts = child.Element.Struct; // rsys.SemanticTypeSystem.GetSemanticTypeStruct(child.Name);
						PropertyInfo piSub = signal.GetType().GetProperty(child.Name);
						object childSignal = piSub.GetValue(signal);

						if (childSignal != null)
						{
							id = ProcessSTS(stfkMap, childsts, childSignal, (sts.Unique || childAsUnique));

							// Associate the ID to this ST's FK for that child table.
							string fieldName = "FK_" + child.Name + "ID";

							if (!stfkMap.ContainsKey(sts))
							{
								stfkMap[sts] = new List<FKValue>();
							}

							stfkMap[sts].Add(new FKValue(fieldName, id, child.UniqueField));
						}
					});

				// Having processed all child ST's, We can now make the same determination of
				// whether the record needs to check for uniqueness, however at this level,
				// we need to write out both ST and any NT values in the current ST structure.
				// This is very similar to an ST without child ST's, but here we also use ST's that are designated as unique to build the composite key.
				if (sts.Unique || childAsUnique)
				{
					// All FK's and NT's of this ST are considered part of the composite key.
					// Get all NT's specifically for this ST (no recursive drilldown)
					List<IFullyQualifiedNativeType> fieldValues = rsys.SemanticTypeSystem.GetFullyQualifiedNativeTypeValues(signal, sts.DeclTypeName, false);
					bool exists = QueryUniqueness(stfkMap, sts, signal, fieldValues, out id, true);

					if (!exists)
					{
						id = Insert(stfkMap, sts, signal);
					}
				}
				else if (sts.SemanticElements.Any(se => se.UniqueField) || sts.NativeTypes.Any(nt => nt.UniqueField))
				{
					// Get only unique NT's specifically for this ST (no recursive drilldown)
					List<IFullyQualifiedNativeType> fieldValues = rsys.SemanticTypeSystem.GetFullyQualifiedNativeTypeValues(signal, sts.DeclTypeName, false).Where(fqnt => fqnt.NativeType.UniqueField).ToList();
					bool exists = QueryUniqueness(stfkMap, sts, signal, fieldValues, out id);

					if (!exists)
					{
						id = Insert(stfkMap, sts, signal);
					}
				}
				else
				{
					// No composite key, so just insert the ST.
					id = Insert(stfkMap, sts, signal);
				}
			}
			else
			{
				// Is this ST designated as unique?
				if (sts.Unique || childAsUnique)
				{
					// If so, then we treat all NT's as a composite unique key.
					// Do lookup to see if the already exists exists.
					// Use all NT fields.  Recurse can be true because we know we don't have any child ST's.
					List<IFullyQualifiedNativeType> fieldValues = rsys.SemanticTypeSystem.GetFullyQualifiedNativeTypeValues(signal, sts.DeclTypeName);
					bool exists = QueryUniqueness(stfkMap, sts, signal, fieldValues, out id);

					if (!exists)
					{
						// If no record, create it.
						id = Insert(stfkMap, sts, signal);
					}
				}
				else if (sts.NativeTypes.Any(nt => nt.UniqueField))
				{
					// If any NT's are designated as unique, then we also have a way of identifying a record.
					// Do lookup to see if the record already exists.
					// Use just the unique fields.
					List<IFullyQualifiedNativeType> fieldValues = rsys.SemanticTypeSystem.GetFullyQualifiedNativeTypeValues(signal, sts.DeclTypeName).Where(fqnt => fqnt.NativeType.UniqueField).ToList();
					bool exists = QueryUniqueness(stfkMap, sts, signal, fieldValues, out id);

					if (!exists)
					{
						// If no record, create it.
						id = Insert(stfkMap, sts, signal);
					}
				}
				else
				{
					// No unique fields, so we just insert the record.
					id = Insert(stfkMap, sts, signal);

					// The ID can now be returned as the FK for any parent ST.
				}
			}

			return id;
		}

		protected int Insert(Dictionary<ISemanticTypeStruct, List<FKValue>> stfkMap, ISemanticTypeStruct sts, object signal)
		{
			// Get native types to insert:
			List<IFullyQualifiedNativeType> ntFieldValues = rsys.SemanticTypeSystem.GetFullyQualifiedNativeTypeValues(signal, sts.DeclTypeName, false);
			StringBuilder sb = new StringBuilder("insert into " + sts.DeclTypeName + " (");
			sb.Append(String.Join(", ", ntFieldValues.Select(f => f.Name)));

			// Get ST's to insert as FK_ID's:
			List<FKValue> fkValues;
			bool hasFKValues = stfkMap.TryGetValue(sts, out fkValues);

			if (hasFKValues && fkValues.Count > 0)
			{
				// Join in the FK_ID field names.
				if (ntFieldValues.Count > 0) sb.Append(", ");
				sb.Append(string.Join(", ", fkValues.Select(fkv => fkv.FieldName)));
			}

			// Setup NT field values:
			sb.Append(") values (");
			sb.Append(String.Join(", ", ntFieldValues.Select(f => "@" + f.Name)));

			// Setup ST FK parameters:
			if (hasFKValues && fkValues.Count > 0)
			{
				if (ntFieldValues.Count > 0) sb.Append(", ");
				sb.Append(string.Join(", ", fkValues.Select(fkv => "@" + fkv.FieldName)));
			}

			sb.Append(")");
			IDbCommand cmd = dbio.CreateCommand();

			// Assign NT values:
			ntFieldValues.ForEach(fv => cmd.Parameters.Add(dbio.CreateParameter(fv.Name, fv.Value)));

			// Assign FK values:
			if (hasFKValues && fkValues.Count > 0)
			{
				fkValues.ForEach(fkv => cmd.Parameters.Add(dbio.CreateParameter(fkv.FieldName, fkv.ID)));
			}

			cmd.CommandText = sb.ToString();
			LogSqlStatement(cmd.CommandText);
			cmd.ExecuteNonQuery();

			int id = dbio.GetLastID(sts.DeclTypeName);

			return id;
		}

		protected bool QueryUniqueness(Dictionary<ISemanticTypeStruct, List<FKValue>> stfkMap, ISemanticTypeStruct sts, object signal, List<IFullyQualifiedNativeType> uniqueFieldValues, out int id, bool allPKs = false)
		{
			id = -1;
			bool ret = false;

			// Get ST's to insert as FK_ID's:
			List<FKValue> fkValues;
			bool hasFKValues = stfkMap.TryGetValue(sts, out fkValues);
			/*
						// If we have no NT's or ST FK's to determine uniqueness (this is valid) then return false: not unique.
						if (uniqueFieldValues.Count == 0 && (!hasFKValues || (hasFKValues && fkValues.Where(fk => fk.UniqueField || allPKs).Count() == 0)))
						{
							return false;
						}
			*/
			StringBuilder sb = new StringBuilder("select id from " + dbio.Delimited(sts.DeclTypeName) + " where ");

			// Put NT fields into "where" clause.
			sb.Append(String.Join(" and ", uniqueFieldValues.Select(f => dbio.Delimited(f.Name) + " = @" + f.Name)));

			// Put unique ST fields into "where" clause.
			if (hasFKValues && fkValues.Any(fk => fk.UniqueField || allPKs))
			{
				if (uniqueFieldValues.Count > 0) sb.Append(" and ");
				sb.Append(String.Join(" and ", fkValues.Where(fk => fk.UniqueField || allPKs).Select(fk => dbio.Delimited(fk.FieldName) + " = @" + fk.FieldName)));
			}

			IDbCommand cmd = dbio.CreateCommand();

			// Populate parameters:
			uniqueFieldValues.ForEach(fv => cmd.Parameters.Add(dbio.CreateParameter(fv.Name, fv.Value)));

			if (hasFKValues && fkValues.Any(fk => fk.UniqueField || allPKs))
			{
				fkValues.Where(fk => fk.UniqueField || allPKs).ForEach(fk => cmd.Parameters.Add(dbio.CreateParameter(fk.FieldName, fk.ID)));
			}

			cmd.CommandText = sb.ToString();
			LogSqlStatement(cmd.CommandText);
			object oid = null;

			try
			{
				oid = cmd.ExecuteScalar();
			}
			catch (Exception ex)
			{
			}

			ret = (oid != null);

			if (ret)
			{
				id = Convert.ToInt32(oid);
			}

			return ret;
		}

		// ------ Query -------

		protected void QueryDatabase(string query)
		{
			try
			{
				List<string> types = query.LeftOf("where").Split(',').Select(s => s.Trim()).ToList();

				// We only have one protocol to query, so we can create the protocol directly since it's already defined.
				if (types.Count() == 1)
				{
					string protocol = types[0];
					AddEmitProtocol(protocol);		// identical protocols are ignored.
					ISemanticTypeStruct sts = rsys.SemanticTypeSystem.GetSemanticTypeStruct(protocol);

					if (!rsys.SemanticTypeSystem.VerifyProtocolExists(protocol))
					{
						throw new Exception("Protocol " + protocol + " is not defined.");
					}

					List<object> signal = QueryType(protocol, String.Empty);

					// Create a carrier for each of the signals in the returned record collection.
					if (UnitTesting)
					{
						signal.ForEach(s => rsys.CreateCarrier(this, sts, s));
					}
					else
					{
						signal.ForEach(s => rsys.CreateCarrierIfReceiver(this, sts, s));
					}
				}
				else if (types.Count() > 1)
				{
					// TODO: Move this into separate functions:

					// Joins require creating dynamic semantic types.
					// Define a new protocol consisting of a root placeholder semantic element with n children,
					// one child for each of the joined semantic types.

					// First we need to find common structures between each of the specified structures.
					Dictionary<string, List<Tuple<ISemanticTypeStruct, ISemanticTypeStruct>>> stSemanticTypes = new Dictionary<string, List<Tuple<ISemanticTypeStruct, ISemanticTypeStruct>>>();

					// For each root type, get all the sub-ST's.
					foreach (string st in types)
					{
						stSemanticTypes[st] = rsys.SemanticTypeSystem.GetAllSemanticTypes(st);
					}

					Dictionary<TypeIntersection, List<ISemanticTypeStruct>> typeIntersectionStructs = new Dictionary<TypeIntersection, List<ISemanticTypeStruct>>();
					List<TypeIntersection> joinOrder = GetJoinOrder(types, stSemanticTypes, typeIntersectionStructs);

					// Since we always start with the first ST in the join list as the base type:
					Dictionary<ISemanticTypeStruct, int> structureUseCounts = new Dictionary<ISemanticTypeStruct, int>();
					List<ISemanticTypeStruct> sharedStructs = typeIntersectionStructs[joinOrder[0]];
					ISemanticTypeStruct sharedStruct = sharedStructs[0];
					string baseType = joinOrder[0].BaseType;

					ISemanticTypeStruct parent0 = stSemanticTypes[baseType].Single(t => t.Item1 == sharedStruct).Item2;
					bool parent0ElementUnique = parent0.SemanticElements.Any(se => se.Name == sharedStruct.DeclTypeName && se.UniqueField);

					// Build the query pieces for the first type:
					ISemanticTypeStruct sts0 = rsys.SemanticTypeSystem.GetSemanticTypeStruct(baseType);
					List<string> fields0 = new List<string>();
					List<string> joins0 = new List<string>();
					BuildQuery(sts0, fields0, joins0, structureUseCounts);

					// Now we're ready to join the other ST's, which are always joinOrder[joinIdx].JoinType.
					for (int joinIdx = 0; joinIdx < joinOrder.Count; joinIdx++)
					{
						string joinType = joinOrder[joinIdx].JoinType;

						// If we've changed the "base" type, then update parent0 and parent0ElementUnique.
						if (joinOrder[joinIdx].BaseType != baseType)
						{
							baseType = joinOrder[joinIdx].BaseType;
							parent0 = stSemanticTypes[baseType].Single(t => t.Item1 == sharedStruct).Item2;
							parent0ElementUnique = parent0.SemanticElements.Any(se => se.Name == sharedStruct.DeclTypeName && se.UniqueField);
						}

						sharedStructs = typeIntersectionStructs[joinOrder[joinIdx]];

						// If the shared structure is a unique field in both parent structures, then we can do then join with the FK_ID's rather than the underlying data.
						// So, for example, in the UniqueKeyJoinQuery unit test, we can join RSSFeedItem and Visited with:
						// "join [one of the tables] on RSSFeedItem.FK_UrlID = Visited.FK_UrlID"		(ignoring aliased table names)
						// IMPORTANT: Where the parent tables in the "on" statement are the parents of the respective shared structure, not the root query structure name (which just so happens to be the same in this case.)

						// If there is NOT a unique key at either or both ends, then we have to drill into all native types at the joined structure level for both query paths, which would look like:
						// "join [one of the tables] on Url1.Value = Url2.Value [and...]" where the and aggregates all the NT values shared between the two query paths.
						// Notice that here it is VITAL that we figure out the aliases for each query path.

						// Interestingly, if both reference is unique structure, we get an intersection.
						// If both reference a non-unique structure, we get an intersection, but then we need to check the parent to see if the element is unique for both paths.

						// TODO: At the moment, we just pick the first shared structure.  At some point we want to pick one that can work with FK's first, then NT unique key values if we can't find an FK join.
						sharedStruct = sharedStructs[0];

						// Find the parent for each root query given the shared structure.
						// TODO: Will "Single" ever barf?
						ISemanticTypeStruct parent1 = stSemanticTypes[joinType].Single(t => t.Item1 == sharedStruct).Item2;
						bool parent1ElementUnique = parent1.SemanticElements.Any(se => se.Name == sharedStruct.DeclTypeName && se.UniqueField);

						// TODO: If there's more than one shared structure, try an pick the one that is unique or who's parent is a unique element.
						// TODO: Write a unit test for this.
						// If the shared structure is unique, or the elements referencing the structure are unique in both parents, then we can use the FK ID between the two parent ST's to join the structures.
						// Otherwise, we have to use the NT values in each structure.
						if ((sharedStruct.Unique) || (parent0ElementUnique && parent1ElementUnique))
						{
							// Build the query pieces for the second type, preserving counts so we don't accidentally re-use an alias.
							ISemanticTypeStruct sts1 = rsys.SemanticTypeSystem.GetSemanticTypeStruct(joinType);
							List<string> fields1 = new List<string>();
							List<string> joins1 = new List<string>();

							BuildQuery(sts1, fields1, joins1, structureUseCounts);
							fields0.AddRange(fields1);

							// Note the root element of the second structure is always aliased as "1".
							// TODO: This doesn't handle self joins.  Scenario?  Unit test?  Test and throw exception?
							// IMPORTANT: In Postgres, we note that the join that declares the table referenced in joins1 must be joined first.
							// TODO: We use a left join here because we want to include records from the first table that may not match with the second table.  This should be user definable, perhaps the way Oracle used to do it with the "+" to indicate a left join rather than an inner join.
							// TODO: The root table name of the second table (parent1) doesn't need an "as" because it will only be referenced once (like in the "from" clause for parent0), however, this means 
							// that we can't join the same type twice.  When will this be an issue?

							// Except for types in the query itself, we need to aliased type.
							string rightSideTableName = parent0.DeclTypeName;

							if (!types.Contains(parent0.DeclTypeName))
							{
								rightSideTableName = parent0.DeclTypeName + "1";		// TODO: But do we need to know which alias, out of a possibility of aliases, to choose from???
							}

							if (sharedStruct.DeclTypeName == parent0.DeclTypeName)
							{
								// The right side should, in this case, be the ID, not an FK, as the left side is joining to the actual table rather than both referencing a common shared FK.
								joins0.Add("left join " + parent1.DeclTypeName + " on " + parent1.DeclTypeName + ".FK_" + sharedStruct.DeclTypeName + "ID = " + rightSideTableName + ".ID");
							}
							else
							{
								joins0.Add("left join " + parent1.DeclTypeName + " on " + parent1.DeclTypeName + ".FK_" + sharedStruct.DeclTypeName + "ID = " + rightSideTableName + ".FK_" + sharedStruct.DeclTypeName + "ID");
							}
							joins0.AddRange(joins1);
						}
						else
						{
							// TODO: Implement a join based on NT unique key values, as we're joining an ST with only NT's.
							throw new Exception("Non-FK joins are currently not supported.");
						}
					}

					string sqlQuery = "select " + String.Join(", ", fields0) + " \r\nfrom " + sts0.DeclTypeName + " \r\n" + String.Join(" \r\n", joins0);

					// Perform the query:
					// TODO: Separate function!

					IDbCommand cmd = dbio.CreateCommand();
					cmd.CommandText = sqlQuery;
					LogSqlStatement(sqlQuery);
					IDataReader reader = cmd.ExecuteReader();

					// Populate the signal with the columns in each record read.
					// Wrap this so we can close the reader if there are any problems.
					try
					{
						while (reader.Read())
						{
							int counter = 0;
							List<object> joinSignals = new List<object>();

							// The resulting fields are in the order of how they're populated based on our join list.
							// Since we're hard-coding a 2 type joins...
							ISemanticTypeStruct outprotocol0 = rsys.SemanticTypeSystem.GetSemanticTypeStruct(types[0]);
							object outsignal0 = rsys.SemanticTypeSystem.Create(types[0]);
							Populate(outprotocol0, outsignal0, reader, ref counter);

							for (int joinIdx = 0; joinIdx < joinOrder.Count; joinIdx++)
							{
								string joinType = joinOrder[joinIdx].JoinType;
								ISemanticTypeStruct outprotocol1 = rsys.SemanticTypeSystem.GetSemanticTypeStruct(joinType);
								object outsignal1 = rsys.SemanticTypeSystem.Create(joinType);

								bool anyNonNull1 = Populate(outprotocol1, outsignal1, reader, ref counter);

								// If all native type values are null in the resulting ST, then this is a join with no record, so set the signal to null.
								if (!anyNonNull1)
								{
									outsignal1 = null;
								}

								joinSignals.Add(outsignal1);
							}

							// Now create a custom type if it doesn't already exist.  The custom type name is formed from the type names in the join.
							string customTypeName = String.Join("_", types);

							if (!rsys.SemanticTypeSystem.VerifyProtocolExists(customTypeName))
							{
								rsys.SemanticTypeSystem.CreateCustomType(customTypeName, types);
								AddEmitProtocol(customTypeName);		// We now emit this custom protocol.
							}

							ISemanticTypeStruct outprotocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct(customTypeName);
							object outsignal = rsys.SemanticTypeSystem.Create(customTypeName);

							// Assign our signals to the children of the custom type.  
							// TODO: Again, self-joins will fail here.
							PropertyInfo pi0 = outsignal.GetType().GetProperty(types[0]);
							pi0.SetValue(outsignal, outsignal0);

							for (int joinIdx = 0; joinIdx < joinOrder.Count; joinIdx++)
							{
								string joinType = joinOrder[joinIdx].JoinType;
								PropertyInfo pi1 = outsignal.GetType().GetProperty(joinType);
								pi1.SetValue(outsignal, joinSignals[joinIdx]);
							}

							// Finally!  Create the carrier:
							if (UnitTesting)
							{
								rsys.CreateCarrier(this, outprotocol, outsignal);
							}
							else
							{
								rsys.CreateCarrierIfReceiver(this, outprotocol, outsignal);
							}
						}
					}
					catch (Exception ex)
					{
						EmitException(ex);
					}
					finally
					{
						reader.Close();
					}
				}
				else
				{
					throw new Exception("Query does not include any semantic types.");
				}
			}
			catch (Exception ex)
			{
				// Anything else that screws up outside of the reader loop.
				EmitException(ex);
			}
		}

		protected List<TypeIntersection> GetJoinOrder(List<string> types, Dictionary<string, List<Tuple<ISemanticTypeStruct, ISemanticTypeStruct>>> stSemanticTypes, Dictionary<TypeIntersection, List<ISemanticTypeStruct>> typeIntersectionStructs)
		{
			// We assume that the first ST is always the "base" ST, and everything else is joined to it or to other ST's.
			// This requires that we process joins 1..n in a specific order to ensure that joins to ST's are first defined, then referenced.
			// TODO: We do not have a test for that.
			List<TypeIntersection> joinOrder = new List<TypeIntersection>();
			List<string> typesToJoin = new List<string>();

			// We need to join all these types.
			// These may become "base" types if we have a dependency like:
			// 1 depends on 2, and 2 depends on 0.
			// To resolve 1, we first discover that 2 depends on 0
			// We then iterate again with 2 as the base type and discover that we can now join 1 as a dependency on 2.
			// The resulting order is then 0, 2, 1.
			typesToJoin.AddRange(types.Skip(1));

			// Assume idx 0 is the base.
			int baseIdx = 0;

			while (typesToJoin.Count > 0)
			{
				int idx = 0;
				bool found = false;

				// Easier to debug if we don't use anonymous methods.  Better for stack traces on exceptions too!
				foreach (string typeToJoin in types)
				{
					// Skip any type that we already found a join for (it won't be in the list.)
					if (!typesToJoin.Contains(typeToJoin))
					{
						++idx;
						continue;
					}

					// Returns a list of intersecting ST's between the base ST and another ST.
					List<ISemanticTypeStruct> sharedStructs = stSemanticTypes[types[baseIdx]].Select(t1 => t1.Item1).Intersect(stSemanticTypes[types[idx]].Select(t2 => t2.Item1)).ToList();

					// If we have shared structure...
					if (sharedStructs.Count > 0)
					{
						// TODO: We still need to verify that we have unique keys in which to accomplish a join.
						// Write a test for this.
						// (For now, we always assume that we do)
						TypeIntersection typeIntersection = new TypeIntersection(types[baseIdx], types[idx]);
						typeIntersectionStructs[typeIntersection] = sharedStructs;
						joinOrder.Add(typeIntersection);
						typesToJoin.Remove(types[idx]);
						found = true;
						// Try next type.
						break;
					}

					++idx;
				}

				if (found)
				{
					// Start with the base again.
					baseIdx = 0;
				}
				else
				{
					// Start with the next possible base and search again.
					++baseIdx;

					// If we've gone through the entire list...
					if (baseIdx >= types.Count)
					{
						// TODO: Determine what type failed to join.
						throw new Exception("Cannot find a common type for the required join.");
					}
				}
			}

			return joinOrder;
		}

		/// <summary>
		/// Return a list of dynamics that represents the semantic element instances in the resulting query set.
		/// </summary>
		protected List<object> QueryType(string protocol, string where)
		{
			List<object> ret = new List<object>();

			// We build the query by recursing through the semantic structure.
			ISemanticTypeStruct sts = rsys.SemanticTypeSystem.GetSemanticTypeStruct(protocol);
			List<string> fields = new List<string>();
			List<string> joins = new List<string>();
			Dictionary<ISemanticTypeStruct, int> structureUseCounts = new Dictionary<ISemanticTypeStruct, int>();
			BuildQuery(sts, fields, joins, structureUseCounts);

			// CRLF for pretty inspection.
			string sqlQuery = "select " + String.Join(", ", fields) + " \r\nfrom " + sts.DeclTypeName + " \r\n" + String.Join(" \r\n", joins);

			IDbCommand cmd = dbio.CreateCommand();
			cmd.CommandText = sqlQuery;
			LogSqlStatement(sqlQuery);
			IDataReader reader = cmd.ExecuteReader();

			// Populate the signal with the columns in each record read.
			while (reader.Read())
			{
				ISemanticTypeStruct outprotocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct(protocol);
				object outsignal = rsys.SemanticTypeSystem.Create(protocol);
				int counter = 0;
				Populate(sts, outsignal, reader, ref counter);
				ret.Add(outsignal);
			}

			reader.Close();

			return ret;
		}

		/// <summary>
		/// Recurses the semantic structure to generate the native type fields and the semantic element joins.
		/// </summary>
		protected void BuildQuery(ISemanticTypeStruct sts, List<string> fields, List<string> joins, Dictionary<ISemanticTypeStruct, int> structureUseCounts)
		{
			// Add native type fields.
			string parentName = GetUseName(sts, structureUseCounts);
			sts.NativeTypes.ForEach(nt => fields.Add(parentName + "." + nt.Name));

			sts.SemanticElements.ForEach(child =>
				{
					ISemanticTypeStruct childsts = child.Element.Struct; // rsys.SemanticTypeSystem.GetSemanticTypeStruct(child.Name);
					IncrementUseCount(childsts, structureUseCounts);
					string asChildName = GetUseName(childsts, structureUseCounts);
					joins.Add("left join " + childsts.DeclTypeName + " as " + asChildName + " on " + asChildName + ".ID = " + parentName + ".FK_" + childsts.DeclTypeName + "ID");
					BuildQuery(childsts, fields, joins, structureUseCounts);
				});
		}

		/// <summary>
		/// Increment the use counter for the structure.
		/// </summary>
		protected int IncrementUseCount(ISemanticTypeStruct childsts, Dictionary<ISemanticTypeStruct, int> structureUseCounts)
		{
			int ret;

			if (!structureUseCounts.TryGetValue(childsts, out ret))
			{
				structureUseCounts[childsts] = 0;
			}

			ret = structureUseCounts[childsts] + 1;
			structureUseCounts[childsts] = ret;

			return ret;
		}

		/// <summary>
		/// Append the use counter if it exists.
		/// </summary>
		protected string GetUseName(ISemanticTypeStruct sts, Dictionary<ISemanticTypeStruct, int> structureUseCounts)
		{
			int count;
			string ret = sts.DeclTypeName;

			if (structureUseCounts.TryGetValue(sts, out count))
			{
				ret = ret + count;
			}

			return ret;
		}

		/// <summary>
		/// Recursively populates the values into the signal.  The recursion algorithm here must match exactly the same
		/// form as the recursion algorithm in BuildQuery, as the correlation between field names and their occurrance
		/// in the semantic structure is relied upon.  For now at least.
		/// Returns true if there are any non-null NT valus.
		/// </summary>
		protected bool Populate(ISemanticTypeStruct sts, object signal, IDataReader reader, ref int parmNumber)
		{
			bool anyNonNull = false;
			List<object> vals = new List<object>();

			for (int i = 0; i < sts.NativeTypes.Count; i++)
			{
				vals.Add(reader[parmNumber++]);
			}

			// No NT's (vals.Count==0) means we just have an ST child, so continue on.
			// We have NT's.  Are they all null?
			//if ( (vals.Count > 0) && (vals.All(v => v == DBNull.Value)) )
			//{
			//	// We don't have a records populate for this query.
			//	ret = null;
			//}
			//else
			{
				// Add native type fields.  Use a foreach loop because ref types can't be used in lambda expressions.
				sts.NativeTypes.ForEachWithIndex((nt, idx) =>
				{
					object val = vals[idx];

					if (val != DBNull.Value)
					{
						Assert.TryCatch(() => nt.SetValue(rsys.SemanticTypeSystem, signal, val), (ex) => EmitException(ex));
						anyNonNull = true;
					}
					else
					{
						// throw new Exception("DBNull is an unsupported native value type.");
						// At the moment, we do nothing because we don't support null field values.
					}
				});

				foreach (ISemanticElement child in sts.SemanticElements)
				{
					ISemanticTypeStruct childsts = child.Element.Struct;
					PropertyInfo piSub = signal.GetType().GetProperty(child.Name);
					object childSignal = piSub.GetValue(signal);
					anyNonNull |= Populate(childsts, childSignal, reader, ref parmNumber);
				}
			}

			return anyNonNull;
		}

		// --------------------

		public void LogSqlStatement(string sql)
		{
			// Test is made for the benefit of unit testing, which doesn't necessarily instantiate this message.
			if (rsys.SemanticTypeSystem.VerifyProtocolExists("LoggerMessage"))
			{
				CreateCarrierIfReceiver("LoggerMessage", signal =>
				{
					signal.MessageTime = DateTime.Now;
					signal.TextMessage.Text.Value = sql;
				});
			}
		}
	}
}


