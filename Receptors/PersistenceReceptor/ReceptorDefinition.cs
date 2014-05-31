using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Clifton.ExtensionMethods;
using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;
using Clifton.Tools.Data;
using Clifton.Tools.Strings.Extensions;

/*
 * Renaming columns in SQLite:
 * begin;
 * PRAGMA writable_schema=1;
 * UPDATE sqlite_master Set SQL=REPLACE(SQL, 'ImageFile', 'ImageFilename') where name = 'APOD';
 * PRAGMA writable_schema=0;
 * commit;
 */

namespace PersistenceReceptor
{
	public class ReceptorDefinition : IReceptorInstance
	{
		public string Name { get { return "SQLite Persistor"; } }
		public bool IsEdgeReceptor { get { return true; } }
		public bool IsHidden { get { return false; } }

		protected IReceptorSystem rsys;
		protected SQLiteConnection conn;
		protected Dictionary<string, Action<dynamic>> protocolActionMap;
		protected Dictionary<string, Action<dynamic>> crudMap;
		const string DatabaseFileName = "hope.db";

		public ReceptorDefinition(IReceptorSystem rsys)
		{
			this.rsys = rsys;
			
			protocolActionMap = new Dictionary<string, Action<dynamic>>();
			protocolActionMap["RequireTable"] = new Action<dynamic>((s) => RequireTable(s));
			protocolActionMap["DatabaseRecord"] = new Action<dynamic>((s) => DatabaseRecord(s));

			crudMap = new Dictionary<string, Action<dynamic>>();
			crudMap["insert"] = new Action<dynamic>((s) => Insert(s));
			crudMap["update"] = new Action<dynamic>((s) => Update(s));
			crudMap["delete"] = new Action<dynamic>((s) => Delete(s));
			crudMap["select"] = new Action<dynamic>((s) => Select(s));

			CreateDBIfMissing();
			OpenDB();
		}

		public string[] GetReceiveProtocols()
		{
			return protocolActionMap.Keys.ToArray();
		}

		public void Initialize()
		{
		}

		public void Terminate()
		{
			conn.Close();
			conn.Dispose();

			// As per this post:
			// http://stackoverflow.com/questions/12532729/sqlite-keeps-the-database-locked-even-after-the-connection-is-closed
			// GC.Collect() is required to ensure that the file handle is released NOW (not when the GC gets a round tuit.  ;)
			GC.Collect();

		}

		public void ProcessCarrier(ICarrier carrier)
		{
			protocolActionMap[carrier.Protocol.DeclTypeName](carrier.Signal);
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

		protected void RequireTable(dynamic signal)
		{
			if (!TableExists(signal.TableName))
			{
				StringBuilder sb = new StringBuilder("create table " + signal.TableName + "(");
				string schema = signal.Schema;

				// Always create a primary key as the field ID. 
				// There is no need to put this into the semantic type definition unless it's required for queries.
				sb.Append("ID INTEGER PRIMARY KEY AUTOINCREMENT");
				List<INativeType> ntypes = rsys.SemanticTypeSystem.GetSemanticTypeStruct(schema).NativeTypes;
				List<ISemanticElement> stypes = rsys.SemanticTypeSystem.GetSemanticTypeStruct(schema).SemanticElements;
				
				// Ignore ID field in the schema, as we specifically create it above.
				// we ignore types, as per the SQLite 3 documentation:
				// "Any column in an SQLite version 3 database, except an INTEGER PRIMARY KEY column, may be used to store a value of any storage class."
				// http://www.sqlite.org/datatype3.html

				var nNames = ntypes.Where(t=>t.Name.ToLower() != "id").Select(t => t.Name);

				// The root semantic type name is the name of the field.
				var sNames = stypes.Where(t => t.Element.Struct.DeclTypeName.ToLower() != "id").Select(t => t.Element.Struct.DeclTypeName);

				var names = nNames.Concat(sNames);

				// We theoretically could have a schema that defines only the ID field.
				if (names.Count() > 0)
				{
					sb.Append(", ");
				}

				string fields = String.Join(", ", names);
				sb.Append(fields);
				sb.Append(");");

				Execute(sb.ToString());
			}
		}

		protected void DatabaseRecord(dynamic signal)
		{
			crudMap[signal.Action.ToLower()](signal);
		}

		protected void Insert(dynamic signal)
		{
			Dictionary<string, object> cvMap = GetColumnValueMap(signal.Row);
			StringBuilder sb = new StringBuilder("insert into " + signal.TableName + "(");
			sb.Append(String.Join(", ", (from c in cvMap where c.Value != null select c.Key).ToArray()));
			sb.Append(") values (");
			sb.Append(String.Join(",", (from c in cvMap where c.Value != null select "@" + c.Key).ToArray()));
			sb.Append(");");

			SQLiteCommand cmd = conn.CreateCommand();
			(from c in cvMap where c.Value != null select c).ForEach(kvp => cmd.Parameters.Add(new SQLiteParameter("@" + kvp.Key, kvp.Value)));
			cmd.CommandText = sb.ToString();
			cmd.ExecuteNonQuery();
			cmd.Dispose();
		}

		protected void Update(dynamic signal)
		{
			Dictionary<string, object> cvMap = GetColumnValueMap(signal.Row);
			StringBuilder sb = new StringBuilder("update " + signal.TableName + " set ");
			sb.Append(String.Join(",", (from c in cvMap where c.Value != null select c.Key + "= @" + c.Key).ToArray()));
			sb.Append(" where " + signal.Where);		// where is required.

			SQLiteCommand cmd = conn.CreateCommand();
			(from c in cvMap where c.Value != null select c).ForEach(kvp => cmd.Parameters.Add(new SQLiteParameter("@" + kvp.Key, kvp.Value)));
			cmd.CommandText = sb.ToString();
			cmd.ExecuteNonQuery();
			cmd.Dispose();
		}

		protected void Delete(dynamic signal)
		{
			// Where clause is optional.
			string sql = "delete from " + signal.TableName;
			if (signal.Where != null) sql = sql + " where " + signal.Where;
			SQLiteCommand cmd = conn.CreateCommand();
			cmd.CommandText = sql;
			cmd.ExecuteNonQuery();
			cmd.Dispose();
		}

		protected void Select(dynamic signal)
		{
			string schema = signal.ResponseProtocol;
			StringBuilder sb = new StringBuilder("select ");

			// TODO: Join these through the common interface IGetSetSemanticType

			// List<INativeType> ntypes = rsys.SemanticTypeSystem.GetSemanticTypeStruct(schema).NativeTypes;
			// List<ISemanticElement> stypes = rsys.SemanticTypeSystem.GetSemanticTypeStruct(schema).SemanticElements;
			// sb.Append(String.Join(", ", (from c in ntypes select c.Name).Concat(from c in stypes select c.Element.Struct.DeclTypeName).ToArray()));

			List<IGetSetSemanticType> types = rsys.SemanticTypeSystem.GetSemanticTypeStruct(schema).AllTypes;
			sb.Append(String.Join(", ", (from c in types select c.Name).ToArray()));

			sb.Append(" from " + signal.TableName);
			if (signal.Where != null) sb.Append(" where " + signal.Where);
			// support for group by is sort of pointless since we're not supporting any mechanism for aggregate functions.
			if (signal.GroupBy != null) sb.Append(" group by " + signal.GroupBy);
			if (signal.OrderBy != null) sb.Append(" order by " + signal.OrderBy);

            SQLiteCommand cmd = conn.CreateCommand();
			cmd.CommandText = sb.ToString();
            SQLiteDataReader reader = cmd.ExecuteReader();

			// Create an instance of the recordset type.
			ISemanticTypeStruct collectionProtocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct(signal.ResponseProtocol + "Recordset");
			dynamic collection = rsys.SemanticTypeSystem.Create(signal.ResponseProtocol + "Recordset");
			collection.Recordset = new List<dynamic>();
			// Return whatever we were sent, so caller can have a reference that it needs.
			collection.Tag = signal.Tag;			

			while (reader.Read())
			{
				ISemanticTypeStruct protocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct(signal.ResponseProtocol);
				dynamic outSignal = rsys.SemanticTypeSystem.Create(signal.ResponseProtocol);
				types.ForEach(t=>t.SetValue(rsys.SemanticTypeSystem, outSignal, reader[t.Name]));

				// Populate the output signal with the fields retrieved from the query, as specified by the requested response protocol
				// ntypes.Cast<IGetSetSemanticType>().ForEach(t => t.SetValue(rsys.SemanticTypeSystem, outSignal, reader[t.Name]));

				// A semantic type is a different beast, potentially with child ST's. 
				// We need to get to the native type parameter to properly initialize this type.
				// Delegate this whole issue to the semantic type itself.
				// An important thing -- schema semantic types can only have one property per type, otherwise we won't know which
				// property to set.
				// stypes.ForEach(t => t.SetValue(rsys.SemanticTypeSystem, outSignal, reader[t.Element.Struct.DeclTypeName]));

				// Add the record to the recordset.
				collection.Recordset.Add(outSignal);

				// rsys.CreateCarrier(this, protocol, outSignal);
			}

			cmd.Dispose();

			// Create the carrier for the recordset.
			rsys.CreateCarrier(this, collectionProtocol, collection);
		}

		protected Dictionary<string, object> GetColumnValueMap(ICarrier carrier)
		{
			// List<INativeType> types = rsys.SemanticTypeSystem.GetSemanticTypeStruct(carrier.Protocol.DeclTypeName).NativeTypes;

			List<IGetSetSemanticType> types = rsys.SemanticTypeSystem.GetSemanticTypeStruct(carrier.Protocol.DeclTypeName).AllTypes;
			Dictionary<string, object> cvMap = new Dictionary<string, object>();
			types.ForEach(t => cvMap[t.Name] = t.GetValue(rsys.SemanticTypeSystem, carrier.Signal));

			return cvMap;
		}

		protected bool TableExists(string tableName)
		{
			string sql = "SELECT name FROM sqlite_master WHERE type='table' AND name=" + tableName.SingleQuote() + ";";
			string name = QueryScalar<string>(sql);

			return tableName == name;
		}

		protected T QueryScalar<T>(string query)
		{
			SQLiteCommand cmd = conn.CreateCommand();
			cmd.CommandText = query;
			T result = (T)cmd.ExecuteScalar();
			cmd.Dispose();

			return result;
		}

		protected void Execute(string sql)
		{
			SQLiteCommand cmd = conn.CreateCommand();
			cmd.CommandText = sql;
			cmd.ExecuteNonQuery();
			cmd.Dispose();
		}
	}
}


