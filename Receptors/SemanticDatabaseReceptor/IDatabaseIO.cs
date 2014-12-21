/*
    Copyright 2104 Higher Order Programming

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

*/

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

		/// <summary>
		/// Used by unit testing.
		/// </summary>
		void DropTable(ISemanticDatabase sdb, string tableName);
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
		string AddLimitClause(string sql, string maxRecords);		// Note that for SQLServer, this must inject "top [x]" after "select".
	}
}
