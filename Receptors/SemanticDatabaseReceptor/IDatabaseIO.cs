using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticDatabaseReceptor
{
	public interface IDatabaseIO
	{
		/// <summary>
		/// For unit test support.
		/// </summary>
		IDbConnection Connection { get; }

		/// <summary>
		/// SQLite Only!
		/// </summary>
		void CreateDBIfMissing(string databaseFilename);

		void CreateTable(ISemanticDatabase sdb, string st, List<Tuple<string, Type>> fieldTypes);
		string GetForeignKeySql(string st, string stField, string childTable, string childField);
		void OpenDB(string connectionString);
		List<string> GetTables(ISemanticDatabase sdb);
		List<string> GetColumns(ISemanticDatabase sdb, string tableName);
		IDbCommand CreateCommand();
		IDbDataParameter CreateParameter(string fieldName, object val);
		void Execute(ISemanticDatabase sdb, string sql);
		void Close();
		int GetLastID(string tableName);
		string Delimited(string name);			// Everybody seems to do it differently.
	}
}
