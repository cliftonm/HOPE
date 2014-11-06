using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clifton.Tools.Strings.Extensions;

namespace SemanticDatabaseReceptor
{
	public class SQLiteIO : IDatabaseIO
	{
		protected SQLiteConnection conn;

		public IDbConnection Connection { get { return conn; } }

		/// <summary>
		/// Return a list of all tables in the database.
		/// </summary>
		public List<string> GetTables(ISemanticDatabase sdb)
		{
			SQLiteCommand cmd = conn.CreateCommand();
			cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table'";
			sdb.LogSqlStatement(cmd.CommandText);
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
		public List<string> GetColumns(ISemanticDatabase sdb, string tableName)
		{
			SQLiteCommand cmd = conn.CreateCommand();
			cmd.CommandText = "pragma table_info('" + tableName + "')";
			sdb.LogSqlStatement(cmd.CommandText);
			SQLiteDataReader reader = cmd.ExecuteReader();
			List<string> columnNames = new List<string>();

			while (reader.Read())
			{
				columnNames.Add(reader[1].ToString());
			}

			return columnNames;
		}

		public void Close()
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

		/// <summary>
		/// Create the database if it doesn't exist.
		/// </summary>
		public void CreateDBIfMissing(string databaseFilename)
		{
			string subPath = Path.GetDirectoryName(databaseFilename);

			if (!File.Exists(databaseFilename))
			{
				SQLiteConnection.CreateFile(databaseFilename);
			}
		}

		public void OpenDB(string connectionString)
		{
			conn = new SQLiteConnection(connectionString);
			conn.Open();
		}

		public void Execute(ISemanticDatabase sdb, string sql)
		{
			SQLiteCommand cmd = conn.CreateCommand();
			cmd.CommandText = sql;
			sdb.LogSqlStatement(cmd.CommandText);
			cmd.ExecuteNonQuery();
			cmd.Dispose();
		}

		public IDbCommand CreateCommand()
		{
			return conn.CreateCommand();
		}

		public IDbDataParameter CreateParameter(string fieldName, object val)
		{
			return new SQLiteParameter("@" + fieldName, val);
		}

		public void CreateTable(ISemanticDatabase sdb, string st, List<Tuple<string, Type>> fieldTypes)
		{
			StringBuilder sb = new StringBuilder("create table " + st + " (");
			List<string> fields = new List<string>();
			fields.Add("ID INTEGER PRIMARY KEY AUTOINCREMENT");
			fields.AddRange(fieldTypes.Select(ft => ft.Item1));
			string fieldList = String.Join(", ", fields);
			sb.Append(fieldList);
			sb.Append(");");
			Execute(sdb, sb.ToString());
		}

		public int GetLastID(string tablename /* ignored */)
		{
			IDbCommand cmd = CreateCommand(); 
			cmd.CommandText = "SELECT last_insert_rowid()";
			int id = Convert.ToInt32(cmd.ExecuteScalar());

			return id;
		}

		public string Delimited(string name)
		{
			return name.Brackets();
		}

		/// <summary>
		/// FK's are an optional feature of SQLite, so this function simply returns null.
		/// </summary>
		/// <returns>null</returns>
		public string GetForeignKeySql(string st, string stField, string childTable, string childField)
		{
			// TODO: At some point, implement this.
			return null;
		}

		/// <summary>
		/// Add SQLite limit clause to the sql string.
		/// </summary>
		public string AddLimitClause(string sql, string maxRecords)
		{
			string ret = sql;

			if (!String.IsNullOrEmpty(maxRecords))
			{
				ret = sql + " limit " + maxRecords;	
			}

			return ret;
		}

		public void DropTable(ISemanticDatabase sdb, string tableName)
		{
			Execute(sdb, "drop table " + tableName);
		}
	}
}
