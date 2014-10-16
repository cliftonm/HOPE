using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticDatabaseReceptor
{
	public interface ISemanticDatabase
	{
		void LogSqlStatement(string sql);
	}
}
