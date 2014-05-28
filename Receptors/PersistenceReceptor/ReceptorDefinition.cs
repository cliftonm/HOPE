using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

namespace PersistenceReceptor
{
	public class ReceptorDefinition : IReceptorInstance
	{
		public string Name { get { return "Persistor"; } }
		public bool IsEdgeReceptor { get { return true; } }
		public bool IsHidden { get { return false; } }

		protected IReceptorSystem rsys;
		protected SQLiteConnection conn;
		protected Dictionary<string, Action<dynamic>> protocolActionMap;
		const string DatabaseFileName = "hope.db";

		public ReceptorDefinition(IReceptorSystem rsys)
		{
			this.rsys = rsys;
			protocolActionMap = new Dictionary<string, Action<dynamic>>();
			protocolActionMap["RequireTable"] = new Action<dynamic>((s) => RequireTable(s));
			protocolActionMap["DatabaseRecord"] = new Action<dynamic>((s) => DatabaseRecord(s));
			CreateDBIfMissing();
			OpenDB();
		}

		public string[] GetReceiveProtocols()
		{
			return protocolActionMap.Keys.ToArray();
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

				// Always create a primary key as the field ID. 
				// There is no need to put this into the semantic type definition unless it's required for queries.
				sb.Append("ID INTEGER PRIMARY KEY AUTOINCREMENT");
				List<INativeType> types = rsys.SemanticTypeSystem.GetSemanticTypeStruct(signal.SemanticTypeName).NativeTypes;
				types.ForEach(t =>
					{
						sb.Append(", ");
						sb.Append(t.Name);
						// we ignore types, as per the SQLite 3 documentation:
						// "Any column in an SQLite version 3 database, except an INTEGER PRIMARY KEY column, may be used to store a value of any storage class."
						// http://www.sqlite.org/datatype3.html
					});
				

				sb.Append(");");

				Execute(sb.ToString());
			}
		}

		protected void DatabaseRecord(dynamic signal)
		{
		}

		protected bool TableExists(string tableName)
		{
			string sql = "SELECT name FROM sqlite_master WHERE type='table' AND name='table_name';";
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


