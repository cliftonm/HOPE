using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
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

// TODO:
//		ST names with spaces need to be replaced with "_"
//		NT names with spaces need to be replaced with "_"
// 		Other issues with naming?  
//			Names that are keywords or other reserved tokens

namespace SemanticDatabase
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

	public class SemanticDatabaseReceptor : BaseReceptor
	{
		public const string DatabaseFileName = "hope_semantic_database.db";

		public override string Name { get { return "Semantic Database"; } }
		public override bool IsEdgeReceptor { get { return true; } }
		public override string ConfigurationUI { get { return "SemanticDatabaseConfig.xml"; } }

		/// <summary>
		/// For serialization only, not displayed on the configuration form.
		/// </summary>
		[UserConfigurableProperty("Internal")]
		public string Protocols { get; set; }

		/// <summary>
		/// Used for unit testing.
		/// </summary>
		public SQLiteConnection Connection { get { return conn; } }

		protected SQLiteConnection conn;
		protected DataTable dt;
		protected DataGridView dgvTypes;

		public SemanticDatabaseReceptor(IReceptorSystem rsys)
			: base(rsys)
		{
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
			CreateDBIfMissing();
			OpenDB();
		}

		/// <summary>
		/// Support for unit testing.
		/// </summary>
		public void ProtocolsUpdated()
		{
			ValidateDatabaseSchema();
			UpdateListeners();
		}

		public override void EndSystemInit()
		{
			base.EndSystemInit();
			ValidateDatabaseSchema();
			UpdateListeners();
		}

		public override void Terminate()
		{
			try
			{
				conn.Close();
				conn.Dispose();
			}
			catch
			{
			}
			finally
			{
				// As per this post:
				// http://stackoverflow.com/questions/12532729/sqlite-keeps-the-database-locked-even-after-the-connection-is-closed
				// GC.Collect() is required to ensure that the file handle is released NOW (not when the GC gets a round tuit.  ;)
				GC.Collect();
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

		/// <summary>
		/// Create the database if it doesn't exist.
		/// </summary>
		protected void CreateDBIfMissing()
		{
			string subPath = Path.GetDirectoryName(DatabaseFileName);

			if (!File.Exists(DatabaseFileName))
			{
				SQLiteConnection.CreateFile(DatabaseFileName);
			}
		}

		protected void OpenDB()
		{
			conn = new SQLiteConnection("Data Source = " + DatabaseFileName);
			conn.Open();
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
				List<string> tableNames = GetTables();
				string[] expectedRootTables = Protocols.Split(';');

				foreach (string expectedRootTable in expectedRootTables)
				{
					CreateIfMissing(expectedRootTable.Trim(), tableNames);
				}
			}
		}

		/// <summary>
		/// Return a list of all tables in the database.
		/// </summary>
		protected List<string> GetTables()
		{
			SQLiteCommand cmd = conn.CreateCommand();
			cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table'";
			LogSqlStatement(cmd.CommandText);
			SQLiteDataReader reader = cmd.ExecuteReader();
			List<string> tableNames = new List<string>();
			
			while (reader.Read())
			{
				tableNames.Add(reader[0].ToString());
			}

			return tableNames;
		}

		/// <summary>
		/// Return a list of all tables in the database.
		/// </summary>
		protected List<string> GetColumns(string tableName)
		{
			SQLiteCommand cmd = conn.CreateCommand();
			cmd.CommandText = "pragma table_info('"+tableName+"')";
			LogSqlStatement(cmd.CommandText);
			SQLiteDataReader reader = cmd.ExecuteReader();
			List<string> columnNames = new List<string>();

			while (reader.Read())
			{
				columnNames.Add(reader[1].ToString());
			}

			return columnNames;
		}

		/// <summary>
		/// Inspects the table list for the specified table.  If not found, it creates the table.
		/// If found, the fields are inspected.
		/// Regardless, the sub-types are recursively inspected as well.
		/// </summary>
		protected void CreateIfMissing(string st, List<string> tableNames)
		{
			if (!tableNames.Contains(st))
			{
				CreateTable(st);
				tableNames.Add(st);
			}
			else
			{
				VerifyColumns(st);
			}

			ISemanticTypeStruct sts = rsys.SemanticTypeSystem.GetSemanticTypeStruct(st);
			sts.SemanticElements.ForEach(child => CreateIfMissing(child.Name, tableNames));
		}

		protected void CreateTable(string st)
		{
			StringBuilder sb = new StringBuilder("create table " + st + " (");
			List<string> fields = new List<string>();
			fields.Add("ID INTEGER PRIMARY KEY AUTOINCREMENT");

			// Create FK's for child SE's.
			// Create fields for NT's.
			ISemanticTypeStruct sts = rsys.SemanticTypeSystem.GetSemanticTypeStruct(st);
			sts.SemanticElements.ForEach(child =>
				{
					fields.Add("FK_" + child.Name + "ID INTEGER");
				});

			// we ignore types, as per the SQLite 3 documentation:
			// "Any column in an SQLite version 3 database, except an INTEGER PRIMARY KEY column, may be used to store a value of any storage class."
			sts.NativeTypes.ForEach(child =>
				{
					fields.Add(child.Name);
					// Type t = child.GetImplementingType(rsys.SemanticTypeSystem);
				});

			string fieldList = String.Join(", ", fields);
			sb.Append(fieldList);
			sb.Append(");");

			Execute(sb.ToString());
		}

		protected void Execute(string sql)
		{
			SQLiteCommand cmd = conn.CreateCommand();
			cmd.CommandText = sql;
			LogSqlStatement(cmd.CommandText);
			cmd.ExecuteNonQuery();
			cmd.Dispose();
		}

		protected void VerifyColumns(string st)
		{
			List<string> colNames = GetColumns(st);										
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
			TableFieldValues tfvEntry=new TableFieldValues(st, sts.Unique);
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
						id = ProcessSTS(stfkMap, childsts, childSignal, (sts.Unique || childAsUnique));

						// Associate the ID to this ST's FK for that child table.
						string fieldName = "FK_" + child.Name + "ID";

						if (!stfkMap.ContainsKey(sts))
						{
							stfkMap[sts] = new List<FKValue>();
						}

						stfkMap[sts].Add(new FKValue(fieldName, id, child.UniqueField));
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
					List<IFullyQualifiedNativeType> fieldValues = rsys.SemanticTypeSystem.GetFullyQualifiedNativeTypeValues(signal, sts.DeclTypeName, false).Where(fqnt=>fqnt.NativeType.UniqueField).ToList();
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
			sb.Append(String.Join(", ", ntFieldValues.Select(f => "@"+f.Name)));

			// Setup ST FK parameters:
			if (hasFKValues && fkValues.Count > 0)
			{
				if (ntFieldValues.Count > 0) sb.Append(", ");
				sb.Append(string.Join(", ", fkValues.Select(fkv => "@" + fkv.FieldName)));
			}

			sb.Append(")");
			SQLiteCommand cmd = conn.CreateCommand();

			// Assign NT values:
			ntFieldValues.ForEach(fv => cmd.Parameters.Add(new SQLiteParameter("@" + fv.Name, fv.Value)));

			// Assign FK values:
			if (hasFKValues && fkValues.Count > 0)
			{
				fkValues.ForEach(fkv => cmd.Parameters.Add(new SQLiteParameter("@" + fkv.FieldName, fkv.ID)));
			}

			cmd.CommandText = sb.ToString();
			LogSqlStatement(cmd.CommandText);
			cmd.ExecuteNonQuery();

			cmd.CommandText = "SELECT last_insert_rowid()";
			int id = Convert.ToInt32(cmd.ExecuteScalar());

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
			StringBuilder sb = new StringBuilder("select id from " + sts.DeclTypeName + " where ");

			// Put NT fields into "where" clause.
			sb.Append(String.Join(" and ", uniqueFieldValues.Select(f => f.Name + " = @" + f.Name)));

			// Put unique ST fields into "where" clause.
			if (hasFKValues && fkValues.Any(fk => fk.UniqueField || allPKs))
			{
				if (uniqueFieldValues.Count > 0) sb.Append(" and ");
				sb.Append(String.Join(" and ", fkValues.Where(fk => fk.UniqueField || allPKs).Select(fk => fk.FieldName + " = @" + fk.FieldName)));
			}

			SQLiteCommand cmd = conn.CreateCommand();

			// Populate parameters:
			uniqueFieldValues.ForEach(fv => cmd.Parameters.Add(new SQLiteParameter("@" + fv.Name, fv.Value)));

			if (hasFKValues && fkValues.Any(fk => fk.UniqueField || allPKs))
			{
				fkValues.Where(fk => fk.UniqueField || allPKs).ForEach(fk => cmd.Parameters.Add(new SQLiteParameter("@" + fk.FieldName, fk.ID)));
			}

			cmd.CommandText = sb.ToString();
			LogSqlStatement(cmd.CommandText);
			object oid = cmd.ExecuteScalar();

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
					signal.ForEach(s => rsys.CreateCarrier(this, sts, s));
				}
				else if (types.Count() > 1)
				{
					// TODO: Move this into separate functions:

					// Joins require creating dynamic semantic types.
					// Define a new protocol consisting of a root placeholder semantic element with n children,
					// one child for each of the joined semantic types.

					// First we need to find common structures between each of the specified structures.
					Dictionary<string, List<Tuple<ISemanticTypeStruct, ISemanticTypeStruct>>> stSemanticTypes = new Dictionary<string, List<Tuple<ISemanticTypeStruct, ISemanticTypeStruct>>>();

					foreach (string st in types)
					{
						stSemanticTypes[st] = rsys.SemanticTypeSystem.GetAllSemanticTypes(st);
					}

					// TODO: We need to implement the logic for discovering intersections with more than 2 joins.
					// TODO: Write a unit test for this scenario.

					List<ISemanticTypeStruct> sharedStructs = stSemanticTypes[types[0]].Select(t1=>t1.Item1).Intersect(stSemanticTypes[types[1]].Select(t2=>t2.Item1)).ToList();

					// If the shared structure is a unique field in both parent structures, then we can do then join with the FK_ID's rather than the underlying data.
					// So, for example, in the UniqueKeyJoinQuery unit test, we can join RSSFeedItem and Visited with:
					// "join [one of the tables] on RSSFeedItem.FK_UrlID = Visited.FK_UrlID"		(ignoring aliased table names)
					// IMPORTANT: Where the parent tables in the "on" statement are the parents of the respective shared structure, not the root query structure name (which just so happens to be the same in this case.)

					// If there is NOT a unique key at either or both ends, then we have to drill into all native types at the joined structure level for both query paths, which would look like:
					// "join [one of the tables] on Url1.Value = Url2.Value [and...]" where the and aggregates all the NT values shared between the two query paths.
					// Notice that here it is VITAL that we figure out the aliases for each query path.

					// Interestingly, if both reference is unique structure, we get an intersection.
					// If both reference a non-unique structure, we get an intersection, but then we need to check the parent to see if the element is unique for both paths.

					if (sharedStructs.Count > 0)
					{
						// TODO: If there's more than one shared structure, try an pick the one that is unique or who's parent is a unique element.
						// TODO: Write a unit test for this.
						if (sharedStructs[0].Unique)
						{
							ISemanticTypeStruct sharedStruct = sharedStructs[0];

							// Find the parent for each root query given the shared structure.
							// TODO: Will "Single" barf?
							ISemanticTypeStruct parent0 = stSemanticTypes[types[0]].Single(t => t.Item1 == sharedStruct).Item2;
							ISemanticTypeStruct parent1 = stSemanticTypes[types[1]].Single(t => t.Item1 == sharedStruct).Item2;

							// Build the query pieces for the first type:
							ISemanticTypeStruct sts0 = rsys.SemanticTypeSystem.GetSemanticTypeStruct(types[0]);
							List<string> fields0 = new List<string>();
							List<string> joins0 = new List<string>();
							Dictionary<ISemanticTypeStruct, int> structureUseCounts = new Dictionary<ISemanticTypeStruct, int>();
							BuildQuery(sts0, fields0, joins0, structureUseCounts);

							// Build the query pieces for the second type, preserving counts so we don't accidentally re-use an alias.
							ISemanticTypeStruct sts1 = rsys.SemanticTypeSystem.GetSemanticTypeStruct(types[1]);
							List<string> fields1 = new List<string>();
							List<string> joins1 = new List<string>();
							BuildQuery(sts1, fields1, joins1, structureUseCounts);

							fields0.AddRange(fields1);
							joins0.AddRange(joins1);

							// Note the root element of the second structure is always aliased as "1".
							// TODO: This doesn't handle self joins.  Scenario?  Unit test?  Test and throw exception?
							joins0.Add("inner join " + parent1.DeclTypeName + " on " + parent1.DeclTypeName + ".FK_" + sharedStructs[0].DeclTypeName + "ID = " + parent0.DeclTypeName + ".FK_" + sharedStructs[0].DeclTypeName + "ID");

							string sqlQuery = "select " + String.Join(", ", fields0) + " \r\nfrom " + sts0.DeclTypeName + " \r\n" + String.Join(" \r\n", joins0);
						}
					}

				}
				else
				{
					throw new Exception("Query does not include any semantic types.");
				}
			}
			catch (Exception ex)
			{
				EmitException(ex);
			}
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

			SQLiteCommand cmd = conn.CreateCommand();
			cmd.CommandText = sqlQuery;
			LogSqlStatement(sqlQuery);
			SQLiteDataReader reader = cmd.ExecuteReader();

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
					int childcount = IncrementUseCount(childsts, structureUseCounts);
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
		/// </summary>
		protected void Populate(ISemanticTypeStruct sts, object signal, SQLiteDataReader reader, ref int parmNumber)
		{
			// Add native type fields.  Use a foreach loop because ref types can't be used in lambda expressions.
			foreach(INativeType nt in sts.NativeTypes)
			{
				try
				{
					nt.SetValue(rsys.SemanticTypeSystem, signal, reader[parmNumber++]);
				}
				catch(Exception ex)
				{
					// TODO: We are silently catching types that we can't convert, such as List<>.
					// We need a unit test for this kind of behavior as well as, of course, the implementation.
					EmitException(ex);
				}
			}

			foreach(ISemanticElement child in sts.SemanticElements)
			{
				ISemanticTypeStruct childsts = child.Element.Struct;
				PropertyInfo piSub = signal.GetType().GetProperty(child.Name);
				object childSignal = piSub.GetValue(signal);
				Populate(childsts, childSignal, reader, ref parmNumber);
			}
		}

		// --------------------

		protected void LogSqlStatement(string sql)
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


