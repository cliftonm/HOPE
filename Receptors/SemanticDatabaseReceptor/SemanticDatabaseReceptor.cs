#define SQLITE
// #define POSTGRES

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

				// SQL commands to generate FK's.
				List<string> fkSql = new List<string>();

				foreach (string expectedRootTable in expectedRootTables)
				{
					CreateIfMissing(expectedRootTable.Trim(), tableNames, fkSql);
				}

				fkSql.Where(s=>!String.IsNullOrEmpty(s)).ForEach(s=>
				{
					dbio.Execute(this, s);
				});
			}
		}

		/// <summary>
		/// Inspects the table list for the specified table.  If not found, it creates the table.
		/// If found, the fields are inspected.
		/// Regardless, the sub-types are recursively inspected as well.
		/// </summary>
		protected void CreateIfMissing(string st, List<string> tableNames, List<string> fkSql)
		{
			// All of these "ToLower's" is because Postgres converts table and field names to lowercase.  
			// Why in the world would it do that???  (Of course, if it's a quoted table or field name, then case is preserved!!!)
			if (!tableNames.Contains(st.ToLower()))
			{
				CreateTable(st, fkSql);
				tableNames.Add(st.ToLower());
			}
			else
			{
				VerifyColumns(st.ToLower());
			}

			ISemanticTypeStruct sts = rsys.SemanticTypeSystem.GetSemanticTypeStruct(st);
			sts.SemanticElements.ForEach(child => CreateIfMissing(child.Name, tableNames, fkSql));
		}

		/// <summary>
		/// Create the table for the specified semantic structure, adding any SQL statements for making foreign key associations.
		/// </summary>
		protected void CreateTable(string st, List<string> fkSql)
		{
			// Fields and their types:
			List<Tuple<string, Type>> fieldTypes = new List<Tuple<string, Type>>();

			// Get the structure object backing the structure name.
			ISemanticTypeStruct sts = rsys.SemanticTypeSystem.GetSemanticTypeStruct(st);

			CreateFkSql(sts, fieldTypes, fkSql);
			CreateNativeTypes(sts, fieldTypes);
			dbio.CreateTable(this, st, fieldTypes);
		}

		/// <summary>
		/// Any reference to a child semantic element is implemented as a foreign key.
		/// Returns any foreign key creation sql statements in fkSql.
		/// </summary>
		protected void CreateFkSql(ISemanticTypeStruct sts, List<Tuple<string, Type>> fieldTypes, List<string> fkSql)
		{
			// Create FK's for child SE's.
			sts.SemanticElements.ForEach(child =>
			{
				string fkFieldName = "FK_" + child.Name + "ID";
				fieldTypes.Add(new Tuple<string, Type>(fkFieldName, typeof(long)));
				fkSql.Add(dbio.GetForeignKeySql(sts.DeclTypeName, fkFieldName, child.Name, "ID"));
			});
		}

		/// <summary>
		/// The supported native types are simple field name - Type tuples.
		/// </summary>
		protected void CreateNativeTypes(ISemanticTypeStruct sts, List<Tuple<string, Type>> fieldTypes)
		{
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
						// TODO: The reason for the try-catch is to deal with implementing types we don't support yet, like List<SomeType>
						// For now, we create a stub type.
						fieldTypes.Add(new Tuple<string, Type>(child.Name, typeof(string)));
					}
				});
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
				object childSignal = GetChildSignal(signal, child);
				CreateTableFieldValueList(tfvList, child.Name, childSignal);
			});

			foreach (INativeType nt in sts.NativeTypes)
			{
				// Acquire value through reflection.
				object val = nt.GetValue(rsys.SemanticTypeSystem, signal);
				tfvEntry.FieldValues.Add(new FieldValue(tfvEntry, nt.Name, val, nt.UniqueField, FieldValueType.NativeType));
			}
		}

		/// <summary>
		/// Drills into any child semantic elements, accumulating foreign keys for each level in the semantic hierarchy.
		/// When all children are inserted/updated, the parent can be inserted.
		/// </summary>
		protected int ProcessSTS(Dictionary<ISemanticTypeStruct, List<FKValue>> stfkMap, ISemanticTypeStruct sts, object signal, bool childAsUnique = false)
		{
			// Drill into each child ST and assign the return ID to this ST's FK for the child table name.
			ProcessChildren(stfkMap, sts, signal, childAsUnique);

			// Having processed all child ST's, We can now make the same determination of
			// whether the record needs to check for uniqueness, however at this level,
			// we need to write out both ST and any NT values in the current ST structure.
			// This is very similar to an ST without child ST's, but here we also use ST's that are designated as unique to build the composite key.
			int id = ArbitrateUniqueness(stfkMap, sts, signal, childAsUnique);

			return id;
		}

		/// <summary>
		/// For each child that has a non-null signal, process its children.
		/// </summary>
		protected void ProcessChildren(Dictionary<ISemanticTypeStruct, List<FKValue>> stfkMap, ISemanticTypeStruct sts, object signal, bool childAsUnique)
		{
			sts.SemanticElements.ForEach(child =>
			{
				// Get the child signal and STS and check it, returning a new or existing ID for the entry.
				ISemanticTypeStruct childsts = child.Element.Struct; // rsys.SemanticTypeSystem.GetSemanticTypeStruct(child.Name);
				object childSignal = GetChildSignal(signal, child);

				// We don't insert null child signals.
				if (childSignal != null)
				{
					int id = ProcessSTS(stfkMap, childsts, childSignal, (sts.Unique || childAsUnique));
					RegisterForeignKeyID(stfkMap, sts, child, id);
				}
			});
		}

		/// <summary>
		/// Based on whether a semantic element is unique or whether the foreign key fields or native types are unique, we determine how to determine uniqueness.
		/// We always perform an insert if there is no way to determine whether the record is unique.
		/// If it is unique, the ID of the existing record is returned.
		/// </summary>
		/// <param name="stfkMap"></param>
		protected int ArbitrateUniqueness(Dictionary<ISemanticTypeStruct, List<FKValue>> stfkMap, ISemanticTypeStruct sts, object signal, bool childAsUnique)
		{
			int id = -1;

			if (sts.Unique || childAsUnique)
			{
				// All FK's and NT's of this ST are considered part of the composite key.
				// Get all NT's specifically for this ST (no recursive drilldown)
				// False here indicates that we only want the native types for this ST -- we aren't recursing into child ST's.
				List<IFullyQualifiedNativeType> fieldValues = rsys.SemanticTypeSystem.GetFullyQualifiedNativeTypeValues(signal, sts.DeclTypeName, false);
				// True indicates that all FK's comprise the composite FK (in addition to unique NT's.)
				id = InsertIfRecordDoesntExist(stfkMap, sts, signal, fieldValues, true);
			}
			else if (sts.SemanticElements.Any(se => se.UniqueField) || sts.NativeTypes.Any(nt => nt.UniqueField))
			{
				// Get only unique NT's specifically for this ST (no recursive drilldown)
				// Note that a unique semantic element will automatically set the unique field for its native type children, subchildren, etc.
				// False here indicates that we only want the native types for this ST -- we aren't recursing into child ST's.
				List<IFullyQualifiedNativeType> fieldValues = rsys.SemanticTypeSystem.GetFullyQualifiedNativeTypeValues(signal, sts.DeclTypeName, false).Where(fqnt => fqnt.NativeType.UniqueField).ToList();
				// False indicates that only those FK's marked as unique comprise the composite FK (in addition to unique NT's.)
				id = InsertIfRecordDoesntExist(stfkMap, sts, signal, fieldValues, false);
			}
			else
			{
				// No SE's or NT's are unique, so just insert the ST, as we cannot make a determination regarding uniqueness.
				id = Insert(stfkMap, sts, signal);
			}

			return id;
		}

		/// <summary>
		/// Insert the record if it doesn't exist.
		/// </summary>
		protected int InsertIfRecordDoesntExist(Dictionary<ISemanticTypeStruct, List<FKValue>> stfkMap, ISemanticTypeStruct sts, object signal, List<IFullyQualifiedNativeType> fieldValues, bool allFKs)
		{
			int id = -1;
			bool exists = QueryUniqueness(stfkMap, sts, signal, fieldValues, out id, allFKs);

			if (!exists)
			{
				id = Insert(stfkMap, sts, signal);
			}

			return id;
		}

		/// <summary>
		/// Given a signal and the child element, returns the value of the child instance.
		/// </summary>
		protected object GetChildSignal(object signal, ISemanticElement child)
		{
			PropertyInfo piSub = signal.GetType().GetProperty(child.Name);
			object childSignal = piSub.GetValue(signal);

			return childSignal;
		}

		protected void SetValue(object signal, string propertyName, object val)
		{
			PropertyInfo pi0 = signal.GetType().GetProperty(propertyName);
			pi0.SetValue(signal, val);
		}

		/// <summary>
		/// Registers a foreign key name and value to be associated with the specified semantic structure, which is used when the ST is inserted
		/// after all child elements have been resolved.
		/// </summary>
		protected void RegisterForeignKeyID(Dictionary<ISemanticTypeStruct, List<FKValue>> stfkMap, ISemanticTypeStruct sts, ISemanticElement child, int id)
		{
			// Associate the ID to this ST's FK for that child table.
			string fieldName = "FK_" + child.Name + "ID";
			CreateKeyIfMissing(stfkMap, sts);
			stfkMap[sts].Add(new FKValue(fieldName, id, child.UniqueField));
		}

		/// <summary>
		/// Creates the key and intializes the value instance if the key is missing from the semantic type foreign key map.
		/// </summary>
		protected void CreateKeyIfMissing(Dictionary<ISemanticTypeStruct, List<FKValue>> stfkMap, ISemanticTypeStruct sts)
		{
			if (!stfkMap.ContainsKey(sts))
			{
				stfkMap[sts] = new List<FKValue>();
			}
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

		/// <summary>
		/// Build and execute a select statement that determines if the record, based on a composite key, already exists.
		/// If so, return the ID of the record.
		/// </summary>
		protected bool QueryUniqueness(Dictionary<ISemanticTypeStruct, List<FKValue>> stfkMap, ISemanticTypeStruct sts, object signal, List<IFullyQualifiedNativeType> uniqueFieldValues, out int id, bool allFKs = false)
		{
			id = -1;
			bool ret = false;
			List<FKValue> fkValues;
			bool hasFKValues = stfkMap.TryGetValue(sts, out fkValues);
			StringBuilder sb = BuildUniqueQueryStatement(hasFKValues, fkValues, sts, uniqueFieldValues, allFKs); 
			IDbCommand cmd = AddParametersToCommand(uniqueFieldValues, hasFKValues, fkValues, allFKs);
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

		/// <summary>
		/// Build the query string to test for uniqueness from NT and SE (FK) unique fields.
		/// </summary>
		protected StringBuilder BuildUniqueQueryStatement(bool hasFKValues, List<FKValue> fkValues, ISemanticTypeStruct sts, List<IFullyQualifiedNativeType> uniqueFieldValues, bool allFKs)
		{
			// Get ST's to insert as FK_ID's:
			StringBuilder sb = new StringBuilder("select id from " + dbio.Delimited(sts.DeclTypeName) + " where ");

			// Put NT fields into "where" clause.
			sb.Append(String.Join(" and ", uniqueFieldValues.Select(f => dbio.Delimited(f.Name) + " = @" + f.Name)));

			// Put unique ST fields into "where" clause.
			if (hasFKValues && fkValues.Any(fk => fk.UniqueField || allFKs))
			{
				if (uniqueFieldValues.Count > 0) sb.Append(" and ");
				sb.Append(String.Join(" and ", fkValues.Where(fk => fk.UniqueField || allFKs).Select(fk => dbio.Delimited(fk.FieldName) + " = @" + fk.FieldName)));
			}

			return sb;
		}

		/// <summary>
		/// Returns a command with unique NT and FK values set in the parameter list.
		/// </summary>
		/// <returns></returns>
		protected IDbCommand AddParametersToCommand(List<IFullyQualifiedNativeType> uniqueFieldValues, bool hasFKValues, List<FKValue> fkValues, bool allFKs)
		{
			IDbCommand cmd = dbio.CreateCommand();

			// Populate parameters:
			uniqueFieldValues.ForEach(fv => cmd.Parameters.Add(dbio.CreateParameter(fv.Name, fv.Value)));

			if (hasFKValues && fkValues.Any(fk => fk.UniqueField || allFKs))
			{
				fkValues.Where(fk => fk.UniqueField || allFKs).ForEach(fk => cmd.Parameters.Add(dbio.CreateParameter(fk.FieldName, fk.ID)));
			}

			return cmd;
		}

		// ------ Query -------

		protected void Preprocess(string query, out string maxRecords, out List<string> types, out List<string> orderBy)
		{
			maxRecords = String.Empty;

			if (query.StartsWith("top"))
			{
				// The # of records.  The query string will be "top [x] [ST]" so we separate out [x] because it's between the first two spaces encountered in the string.
				maxRecords = query.Between(' ', ' ');
				query = query.RightOf(' ').RightOf(' ');		// remove the "top [x] "
			}

			// Types are to the left of any were and order by's.
			types = query.LeftOf(" where ").LeftOf(" order by ").Split(',').Select(s => s.Trim()).ToList();
			orderBy = new List<string>();
			string strOrderBy = query.RightOf(" order by ");

			if (!String.IsNullOrEmpty(strOrderBy))
			{
				orderBy.AddRange(strOrderBy.Split(',').Select(s => s.Trim()));
			}
		}

		protected void EmitSignals(List<object> signals, ISemanticTypeStruct sts)
		{
			// Create a carrier for each of the signals in the returned record collection.
			if (UnitTesting)
			{
				signals.ForEach(s => rsys.CreateCarrier(this, sts, s));
			}
			else
			{
				signals.ForEach(s => rsys.CreateCarrierIfReceiver(this, sts, s));
			}
		}

		/// <summary>
		/// Query is of the form [top n] ST [,ST] [where {where clause}] [order by {ST [,ST]}
		/// The ST's in the where and order by must resolve to single NT elements, otherwise they must be fully qualified ST.NT names.
		/// </summary>
		/// <param name="query"></param>
		protected void QueryDatabase(string query)
		{
			try
			{
				string maxRecords = null;
				List<string> types;
				List<string> orderBy;

				Preprocess(query, out maxRecords, out types, out orderBy);

				// We only have one protocol to query, so we can create the protocol directly since it's already defined.
				if (types.Count() == 1)
				{
					string protocol = types[0];
					AddEmitProtocol(protocol);		// identical protocols are ignored.
					ISemanticTypeStruct sts = rsys.SemanticTypeSystem.GetSemanticTypeStruct(protocol);
					List<object> signals = QueryType(protocol, String.Empty, orderBy, maxRecords);
					EmitSignals(signals, sts);
				}
				else if (types.Count() > 1)
				{
					// Joins require creating dynamic semantic types.
					// Define a new protocol consisting of a root placeholder semantic element with n children,
					// one child for each of the joined semantic types.

					// First we need to find common structures between each of the specified structures.
					Dictionary<string, List<Tuple<ISemanticTypeStruct, ISemanticTypeStruct>>> stSemanticTypes = new Dictionary<string, List<Tuple<ISemanticTypeStruct, ISemanticTypeStruct>>>();
					Dictionary<TypeIntersection, List<ISemanticTypeStruct>> typeIntersectionStructs = new Dictionary<TypeIntersection, List<ISemanticTypeStruct>>();
					Dictionary<ISemanticTypeStruct, int> structureUseCounts = new Dictionary<ISemanticTypeStruct, int>();

					List<TypeIntersection> joinOrder = DiscoverJoinOrder(types, stSemanticTypes, typeIntersectionStructs);

					// Since we always start with the first ST in the join list as the base type:
					List<ISemanticTypeStruct> sharedStructs = typeIntersectionStructs[joinOrder[0]];
					ISemanticTypeStruct sharedStruct = sharedStructs[0];
					string baseType = joinOrder[0].BaseType;

					ISemanticTypeStruct parent0 = stSemanticTypes[baseType].First(t => t.Item1 == sharedStruct).Item2;
					bool parent0ElementUnique = parent0.SemanticElements.Any(se => se.Name == sharedStruct.DeclTypeName && se.UniqueField);

					// Build the query pieces for the first type:
					ISemanticTypeStruct sts0 = rsys.SemanticTypeSystem.GetSemanticTypeStruct(baseType);
					List<string> fields0 = new List<string>();
					List<string> joins0 = new List<string>();
					List<Tuple<string, string>> fqntAliases = new List<Tuple<string, string>>();

					BuildQuery(sts0, fields0, joins0, structureUseCounts, sts0.DeclTypeName, fqntAliases);

					// Now we're ready to join the other ST's, which are always joinOrder[joinIdx].JoinType.
					for (int joinIdx = 0; joinIdx < joinOrder.Count; joinIdx++)
					{
						string joinType = joinOrder[joinIdx].JoinType;
						FixupBaseType(stSemanticTypes, sharedStruct, joinOrder, joinIdx, ref baseType, ref parent0, ref parent0ElementUnique);

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
						// TODO: If there's more than one shared structure, try an pick the one that is unique or who's parent is a unique element.
						// TODO: Write a unit test for this.
						sharedStruct = sharedStructs[0];

						// Find the parent for each root query given the shared structure.
						// TODO: Will "Single" ever barf?
						ISemanticTypeStruct parent1 = stSemanticTypes[joinType].First(t => t.Item1 == sharedStruct).Item2;

						// The shared struct may not be the immediate child of parent0, therefore we need to drill into parent0 to find this child.
						// A good example is the query: 
						// top 40 RSSFeedBookmark, RSSFeedItem, UrlVisited, RSSFeedItemDisplayed order by RSSFeedPubDate desc, RSSFeedName
						// We note that this join:
						// left join UrlVisited on UrlVisited.FK_UrlID = RSSFeedBookmark.FK_UrlID
						// is incorrect.  It needs to be:
						// left join UrlVisited on UrlVisited.FK_UrlID = RSSFeedUrl1.FK_UrlID
						// as this is the parent of "Url", which is the shared structure in this case.
						// TODO: Write a unit test for this case.

						ISemanticTypeStruct parentSub0 = parent0;

						// If we're joining on an ID (parent0 == sharedStruct) then ignore this step.
						if (parent0 != sharedStruct)		
						{
							parentSub0 = parent0.SemanticElementContaining(sharedStruct);
						}

						bool parent1ElementUnique = parent1.SemanticElements.Any(se => se.Name == sharedStruct.DeclTypeName && se.UniqueField);

						// If the shared structure is unique, or the elements referencing the structure are unique in both parents, then we can use the FK ID between the two parent ST's to join the structures.
						// Otherwise, we have to use the NT values in each structure.
						if ((sharedStruct.Unique) || (parent0ElementUnique && parent1ElementUnique))
						{
							// Build the query pieces for the second type, preserving counts so we don't accidentally re-use an alias.
							ISemanticTypeStruct sts1 = rsys.SemanticTypeSystem.GetSemanticTypeStruct(joinType);
							List<string> fields1 = new List<string>();
							List<string> joins1 = new List<string>();

							BuildQuery(sts1, fields1, joins1, structureUseCounts, sts1.DeclTypeName, fqntAliases);
							fields0.AddRange(fields1);

							// Note the root element of the second structure is always aliased as "1".
							// TODO: This doesn't handle self joins.  Scenario?  Unit test?  Test and throw exception?
							// IMPORTANT: In Postgres, we note that the join that declares the table referenced in joins1 must be joined first.
							// TODO: We use a left join here because we want to include records from the first table that may not match with the second table.  This should be user definable, perhaps the way Oracle used to do it with the "+" to indicate a left join rather than an inner join.
							// TODO: The root table name of the second table (parent1) doesn't need an "as" because it will only be referenced once (like in the "from" clause for parent0), however, this means 
							// that we can't join the same type twice.  When will this be an issue?

							// Except for types in the query itself, we need to aliased type.
							string rightSideTableName = parentSub0.DeclTypeName;

							if (!types.Contains(parentSub0.DeclTypeName))
							{
								rightSideTableName = parentSub0.DeclTypeName + "1";		// TODO: But do we need to know which alias, out of a possibility of aliases, to choose from???
							}

							if (sharedStruct.DeclTypeName == parentSub0.DeclTypeName)
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
					sqlQuery = sqlQuery + " " + ParseOrderBy(orderBy, fqntAliases);
					sqlQuery = dbio.AddLimitClause(sqlQuery, maxRecords);

					ReadResults(sqlQuery, types, joinOrder);
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

		private void ReadResults(string sqlQuery, List<string> types, List<TypeIntersection> joinOrder)
		{
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
					object outsignal0 = PopulateStructure(types[0], reader, ref counter);
					PopulateJoinStructures(joinSignals, joinOrder, reader, ref counter);

					// Now create a custom type if it doesn't already exist.  The custom type name is formed from the type names in the join.
					ISemanticTypeStruct outprotocol;
					object outsignal = CreateCustomType(types, out outprotocol);

					// Assign our signals to the children of the custom type.  
					// TODO: Again, self-joins will fail here.
					SetValue(outsignal, types[0], outsignal0);
					SetJoinedSignals(outsignal, joinOrder, joinSignals);

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

		protected void SetJoinedSignals(object outsignal, List<TypeIntersection> joinOrder, List<object> joinSignals)
		{
			for (int joinIdx = 0; joinIdx < joinOrder.Count; joinIdx++)
			{
				string joinType = joinOrder[joinIdx].JoinType;
				SetValue(outsignal, joinType, joinSignals[joinIdx]);
			}
		}

		protected void PopulateJoinStructures(List<object> joinSignals, List<TypeIntersection> joinOrder, IDataReader reader, ref int counter)
		{
			for (int joinIdx = 0; joinIdx < joinOrder.Count; joinIdx++)
			{
				string joinType = joinOrder[joinIdx].JoinType;
				object outsignal1 = PopulateStructure(joinType, reader, ref counter);
				joinSignals.Add(outsignal1);
			}
		}

		/// <summary>
		/// Create, if it doesn't exist, the custom type to handle the resulting joined results.
		/// </summary>
		protected object CreateCustomType(List<string> types, out ISemanticTypeStruct outprotocol)
		{
			string customTypeName = String.Join("_", types);

			if (!rsys.SemanticTypeSystem.VerifyProtocolExists(customTypeName))
			{
				rsys.SemanticTypeSystem.CreateCustomType(customTypeName, types);
				AddEmitProtocol(customTypeName);		// We now emit this custom protocol.
			}

			outprotocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct(customTypeName);
			object outsignal = rsys.SemanticTypeSystem.Create(customTypeName);

			return outsignal;
		}

		/// <summary>
		/// Populate the structure at the specified index, setting the return signal to null if all fields for that structure are null.
		/// </summary>
		protected object PopulateStructure(string type, IDataReader reader, ref int counter)
		{
			ISemanticTypeStruct outprotocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct(type);
			object outsignal = rsys.SemanticTypeSystem.Create(type);
			bool anyNonNull = Populate(outprotocol, outsignal, reader, ref counter);

			if (!anyNonNull)
			{
				outsignal = null;
			}

			return outsignal;
		}

		protected void FixupBaseType(Dictionary<string, List<Tuple<ISemanticTypeStruct, ISemanticTypeStruct>>> stSemanticTypes, ISemanticTypeStruct sharedStruct, List<TypeIntersection> joinOrder, int joinIdx, ref string baseType, ref ISemanticTypeStruct parent0, ref bool parent0ElementUnique)
		{
			// If we've changed the "base" type, then update parent0 and parent0ElementUnique.
			if (joinOrder[joinIdx].BaseType != baseType)
			{
				baseType = joinOrder[joinIdx].BaseType;
				parent0 = stSemanticTypes[baseType].First(t => t.Item1 == sharedStruct).Item2;
				parent0ElementUnique = parent0.SemanticElements.Any(se => se.Name == sharedStruct.DeclTypeName && se.UniqueField);
			}
		}

		protected List<TypeIntersection> DiscoverJoinOrder(List<string> types, Dictionary<string, List<Tuple<ISemanticTypeStruct, ISemanticTypeStruct>>> stSemanticTypes, Dictionary<TypeIntersection, List<ISemanticTypeStruct>> typeIntersectionStructs)
		{
			// For each root type, get all the sub-ST's.
			foreach (string st in types)
			{
				stSemanticTypes[st] = rsys.SemanticTypeSystem.GetAllSemanticTypes(st);
			}

			List<TypeIntersection> joinOrder = GetJoinOrder(types, stSemanticTypes, typeIntersectionStructs);

			return joinOrder;
		}

		/// <summary>
		/// Iterate until all joins are resolved.
		/// </summary>
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

			// Do we have any types left to join?
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

					// Useful for debugging, we get the semantic struct names as strings that intersect the two types.
					List<string> sharedStructTypeNames = stSemanticTypes[types[baseIdx]].Select(t1 => t1.Item1.DeclTypeName).Intersect(stSemanticTypes[types[idx]].Select(t2 => t2.Item1.DeclTypeName)).ToList();

					// Returns a list of intersecting ST's between the base ST and another ST where the struct itself has semantic elements, as these can be joined using their FK's.
					List<ISemanticTypeStruct> sharedStructs = stSemanticTypes[types[baseIdx]].Select(t1 => t1.Item1).Intersect(stSemanticTypes[types[idx]].Select(t2 => t2.Item1)).Where(st => st.SemanticElements.Count > 0).ToList();

					if (sharedStructs.Count == 0)
					{
						// Loosen our requirement, as we can include structs with no SE sub-types but whose parent elements are both designated as unique.
						sharedStructs = stSemanticTypes[types[baseIdx]].Select(t1 => t1.Item1).Intersect(stSemanticTypes[types[idx]].Select(t2 => t2.Item1)).ToList();
					}

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
					else
					{
						// TODO -- implement finding a shared struct of native types.  This is a looser qualifier (removing the Where clause):
						// List<ISemanticTypeStruct> sharedStructs = stSemanticTypes[types[baseIdx]].Select(t1 => t1.Item1).Intersect(stSemanticTypes[types[idx]].Select(t2 => t2.Item1)).ToList();
						// However, currently, we don't implement this behavior -- no unit tests!
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
					// TODO: Determine what type failed to join so we can put out a more intelligent exception.
					throw new Exception("Cannot find a common type for the required join.");
				}
			}

			return joinOrder;
		}

		/// <summary>
		/// Return a list of objects that represents the semantic element instances (signals) in the resulting query set.
		/// </summary>
		protected List<object> QueryType(string protocol, string where, List<string> orderBy, string maxRecords)
		{
			// We build the query by recursing through the semantic structure.
			ISemanticTypeStruct sts = rsys.SemanticTypeSystem.GetSemanticTypeStruct(protocol);
			List<string> fields = new List<string>();
			List<string> joins = new List<string>();
			Dictionary<ISemanticTypeStruct, int> structureUseCounts = new Dictionary<ISemanticTypeStruct, int>();
			List<Tuple<string, string>> fqntAliases = new List<Tuple<string, string>>();
			BuildQuery(sts, fields, joins, structureUseCounts, sts.DeclTypeName, fqntAliases);
			string sqlQuery = CreateSqlStatement(sts, fields, joins, fqntAliases, where, orderBy, maxRecords);
			List<object> ret = PopulateSignals(sqlQuery, sts);

			return ret;
		}

		protected List<object> PopulateSignals(string sqlQuery, ISemanticTypeStruct sts)
		{
			List<object> ret = new List<object>();
			IDataReader reader = AcquireReader(sqlQuery);

			while (reader.Read())
			{
				object outsignal = rsys.SemanticTypeSystem.Create(sts.DeclTypeName);
				int counter = 0;		// For a single table join, counter is always 0.
				// Populate the signal with the columns in each record read.
				Populate(sts, outsignal, reader, ref counter);
				ret.Add(outsignal);
			}

			reader.Close();

			return ret;
		}

		/// <summary>
		/// Return a reader for the specified query.
		/// </summary>
		protected IDataReader AcquireReader(string sqlQuery)
		{
			IDbCommand cmd = dbio.CreateCommand();
			cmd.CommandText = sqlQuery;
			LogSqlStatement(sqlQuery);
			IDataReader reader = cmd.ExecuteReader();

			return reader;
		}

		protected string CreateSqlStatement(ISemanticTypeStruct sts, List<string> fields, List<string> joins, List<Tuple<string, string>> fqntAliases, string where, List<string> orderBy, string maxRecords)
		{
			// CRLF for pretty inspection.
			string sqlQuery = "select " + String.Join(", ", fields) + " \r\nfrom " + sts.DeclTypeName + " \r\n" + String.Join(" \r\n", joins);
			sqlQuery = sqlQuery + " " + ParseOrderBy(orderBy, fqntAliases);
			sqlQuery = dbio.AddLimitClause(sqlQuery, maxRecords);

			return sqlQuery;
		}

		protected string ParseOrderBy(List<string> orderBy, List<Tuple<string, string>> fqntAliases)
		{
			StringBuilder sb = new StringBuilder();

			if (orderBy.Count > 0)
			{
				sb.Append("\r\norder by ");
				List<string> fields = new List<string>();

				foreach (string orderByField in orderBy)
				{
					// Find a match in the FQN - alias map.
					// We expect to find one and only one match.
					try
					{
						// strip off asc/desc
						string fieldName = orderByField.LeftOf(" asc").LeftOf(" desc");
						string alias = fqntAliases.First(fqnt => fqnt.Item1.Contains(fieldName)).Item2;

						if (fieldName != orderByField)
						{
							// Put back any "asc" or "desc" descriptor, which is always to the right of the field name separated by a space.
							alias = alias + " " + orderByField.RightOf(' ');
						}

						fields.Add(alias);
					}
					catch
					{
						// Provide a more useful exception to the user rather than the one .NET will throw.
						// TODO: Should this be a user popup error?
						EmitException("The order by field " + orderByField + " cannot be resolved to a single native type.  It requires further specification to disambiguate from other semantic types.");
					}
				}

				sb.Append(String.Join(", ", fields));
			}

			return sb.ToString();
		}

		/// <summary>
		/// Recurses the semantic structure to generate the native type fields and the semantic element joins.
		/// fqntAliases -- fully qualified native type and it's actual alias in the field list.
		/// </summary>
		protected void BuildQuery(ISemanticTypeStruct sts, List<string> fields, List<string> joins, Dictionary<ISemanticTypeStruct, int> structureUseCounts, string fqn, List<Tuple<string, string>> fqntAliases)
		{
			// Add native type fields.
			string parentName = GetUseName(sts, structureUseCounts);
			sts.NativeTypes.ForEach(nt =>
				{
					string qualifiedFieldName = fqn + "." + nt.Name;
					string qualifiedAliasFieldName = parentName + "." + nt.Name;
					fields.Add(qualifiedAliasFieldName);
					fqntAliases.Add(new Tuple<string, string>(qualifiedFieldName, qualifiedAliasFieldName));
				});

			sts.SemanticElements.ForEach(child =>
				{
					ISemanticTypeStruct childsts = child.Element.Struct; // rsys.SemanticTypeSystem.GetSemanticTypeStruct(child.Name);
					IncrementUseCount(childsts, structureUseCounts);
					string asChildName = GetUseName(childsts, structureUseCounts);
					joins.Add("left join " + childsts.DeclTypeName + " as " + asChildName + " on " + asChildName + ".ID = " + parentName + ".FK_" + childsts.DeclTypeName + "ID");
					BuildQuery(childsts, fields, joins, structureUseCounts, fqn+"."+childsts.DeclTypeName, fqntAliases);
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
					object childSignal = GetChildSignal(signal, child);
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


