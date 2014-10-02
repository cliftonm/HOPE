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

namespace SemanticDatabase
{
	public enum FieldValueType
	{
		NativeType,
		SemanticType,
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
		const string DatabaseFileName = "hope_semantic_database.db";

		public override string Name { get { return "Semantic Database"; } }
		public override bool IsEdgeReceptor { get { return true; } }
		public override string ConfigurationUI { get { return "SemanticDatabaseConfig.xml"; } }

		/// <summary>
		/// For serialization only, not displayed on the configuration form.
		/// </summary>
		[UserConfigurableProperty("Internal")]
		public string Protocols { get; set; }
		
		protected SQLiteConnection conn;
		protected DataTable dt;
		protected DataGridView dgvTypes;

		public SemanticDatabaseReceptor(IReceptorSystem rsys)
			: base(rsys)
		{
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
			string st = carrier.Protocol.DeclTypeName;
			List<TableFieldValues> tfvList = CreateTableFieldValueList(st, carrier.Signal);
			List<TableFieldValues> tfvUniqueFielddList = tfvList.Where(t => t.UniqueField || t.FieldValues.Any(fv=>fv.UniqueField && fv.Type==FieldValueType.NativeType)).ToList();
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
					row[0] = tp;
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
			List<string> tableNames = GetTables();
			string[] expectedRootTables = Protocols.Split(';');

			foreach (string expectedRootTable in expectedRootTables)
			{
				CreateIfMissing(expectedRootTable, tableNames);
			}
		}

		/// <summary>
		/// Return a list of all tables in the database.
		/// </summary>
		protected List<string> GetTables()
		{
			SQLiteCommand cmd = conn.CreateCommand();
			cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table'";
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
			// TODO: Remove protocols that are not being listened to anymore
			Protocols.Split(';').ForEach(p => AddReceiveProtocol(p));			
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
			TableFieldValues tfvEntry=new TableFieldValues(st, sts.UniqueField);
			tfvList.Add(tfvEntry);

			sts.SemanticElements.ForEach(child =>
			{
				string fieldName = "FK_" + child.Name + "ID";
				tfvEntry.FieldValues.Add(new FieldValue(tfvEntry, fieldName, null, child.Element.Struct.UniqueField, FieldValueType.SemanticType));
				PropertyInfo piSub = signal.GetType().GetProperty(child.Name);
				object childSignal = piSub.GetValue(signal);
				CreateTableFieldValueList(tfvList, child.Name, childSignal);
			});

			foreach (INativeType nt in sts.NativeTypes)
			{
				// Acquire value through reflection.
				PropertyInfo pi = signal.GetType().GetProperty(nt.Name);
				object val = pi.GetValue(signal);
				tfvEntry.FieldValues.Add(new FieldValue(tfvEntry, nt.Name, val, nt.UniqueField, FieldValueType.NativeType));
			}
		}
	}
}


