using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clifton.Tools.Strings.Extensions;

using Npgsql;

namespace SemanticDatabaseReceptor
{
	public class PostgresIO : IDatabaseIO
	{
		protected NpgsqlConnection conn;

		public IDbConnection Connection { get { return conn; } }

		/// <summary>
		/// Return a list of all tables in the database.
		/// </summary>
		public List<string> GetTables(ISemanticDatabase sdb)
		{
			NpgsqlCommand cmd = conn.CreateCommand();
			cmd.CommandText = "SELECT table_name FROM information_schema.tables WHERE table_schema='public'";
			sdb.LogSqlStatement(cmd.CommandText);
			NpgsqlDataReader reader = cmd.ExecuteReader();
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
			NpgsqlCommand cmd = conn.CreateCommand();
			// currval() only works after an INSERT (which has executed nextval() ), in the same session.
			cmd.CommandText = "select column_name, data_type, character_maximum_length from INFORMATION_SCHEMA.COLUMNS where table_name = " + tableName.SingleQuote();
			sdb.LogSqlStatement(cmd.CommandText);
			NpgsqlDataReader reader = cmd.ExecuteReader();
			List<string> columnNames = new List<string>();

			while (reader.Read())
			{
				columnNames.Add(reader[0].ToString());
			}

			return columnNames;
		}

		public void Close()
		{
			conn.Close();
		}

		/// <summary>
		/// Create the database if it doesn't exist.
		/// </summary>
		public void CreateDBIfMissing(string databaseFilename)
		{
			// TODO: Implementation.
		}

		public void OpenDB(string connectionString)
		{
			// TODO: Implementation.
			conn = new NpgsqlConnection(connectionString);
			conn.Open();
		}

		public void Execute(ISemanticDatabase sdb, string sql)
		{
			NpgsqlCommand cmd = conn.CreateCommand();
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
			return new NpgsqlParameter("@" + fieldName, val);
		}

		public void CreateTable(ISemanticDatabase sdb, string st, List<Tuple<string, Type>> fieldTypes)
		{
			Dictionary<Type, string> typeMap = new Dictionary<Type, string>()
			{
				{typeof(int), "integer"},
				{typeof(long), "int8"},
				{typeof(bool), "boolean"},
				{typeof(string), "text"},
				{typeof(char), "char"},
				{typeof(float), "float8"},
				{typeof(double), "float8"},
				{typeof(decimal), "decimal"},
				{typeof(DateTime), "timestamptz"},
			};

			StringBuilder sb = new StringBuilder("create table " + st + " (");
			List<string> fields = new List<string>();
			fields.Add("ID SERIAL PRIMARY KEY");					// Everybody does it differently!

			fieldTypes.ForEach(ft =>
				{
					string typeName;
					
					if (!typeMap.TryGetValue(ft.Item2, out typeName))
					{
						throw new Exception("Type " + ft.Item2.ToString() + " not supported.");
					}

					fields.Add(ft.Item1 + " " + typeName);
				});

			string fieldList = String.Join(", ", fields);
			sb.Append(fieldList);
			sb.Append(");");
			Execute(sdb, sb.ToString());
		}

		public int GetLastID(string tableName)
		{
			IDbCommand cmd = CreateCommand();
			cmd.CommandText = "SELECT currval(pg_get_serial_sequence(" + tableName.SingleQuote() + ", 'id'))";
			int id = Convert.ToInt32(cmd.ExecuteScalar());

			return id;
		}

		public string Delimited(string name)
		{
			// Note that we also have to conver the name to lowercase, because "Foobar" is not the same as foobar.
			return name.ToLower().Quote();
		}

		public string GetForeignKeySql(string st, string stField, string childTable, string childField)
		{
			return "alter table " + st + " add foreign key (" + stField + ") references " + childTable + "(" + childField + ")";
		}

		public string AddLimitClause(string sql, string maxRecords)
		{
			string ret = sql;

			if (!String.IsNullOrEmpty(maxRecords))
			{
				ret = sql + " limit " + maxRecords;
			}

			return ret;
		}
	}
}
