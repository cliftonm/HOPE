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
using System.Linq;
using System.Text;
using System.Windows.Forms;

using TypeSystemExplorer.Controls;
using TypeSystemExplorer.Controllers;
using TypeSystemExplorer.Models;

using Clifton.SemanticTypeSystem;
using Clifton.SemanticTypeSystem.Interfaces;
using Clifton.Tools.Data;
using Clifton.Tools.Strings.Extensions;

namespace TypeSystemExplorer.Views
{
	public class SymbolTableView : UserControl
	{
		public ApplicationModel Model { get; protected set; }
		public ApplicationFormController ApplicationController { get; protected set; }
		public SymbolTableController Controller { get; protected set; }
		public DataGridViewControl SymbolTableDataGrid { get; protected set; }

		/// <summary>
		/// Recursively populates the symbol table, starting with the topmost elements.
		/// </summary>
		public void UpdateSymbolTable(List<SemanticTypeInstance> symbolTable)
		{
			DataTable dt = CreateSymbolDataTable();
			PopulateLevel(dt, symbolTable, null, "");
			SymbolTableDataGrid.DataSource = dt;			
		}

		public void Clear()
		{
			SymbolTableDataGrid.DataSource = null;
		}

		/// <summary>
		/// Create the data table structure for the grid.
		/// </summary>
		protected DataTable CreateSymbolDataTable()
		{
			DataTable dt = new DataTable();
			dt.Columns.Add(new DataColumn("Symbol"));
			dt.Columns.Add(new DataColumn("Value"));

			return dt;
		}

		/// <summary>
		/// Add tree nodes, recursing into children.
		/// </summary>
		protected void PopulateLevel(DataTable dt, List<SemanticTypeInstance> symbolTable, object parent, string indentation)
		{
			foreach (SemanticTypeInstance sti in symbolTable.Where(t => t.Parent == parent))
			{
				AddSymbol(dt, sti, indentation);
				PopulateLevel(dt, symbolTable, sti.Instance, indentation + "  ");
			}
		}

		/// <summary>
		/// Add a row to the symbol table, including the value if the ST maps to a type.
		/// </summary>
		protected void AddSymbol(DataTable dt, SemanticTypeInstance sti, string indentation)
		{
			DataRow row = dt.NewRow();
			row[0] = indentation + sti.Name;

			if (sti.Definition.Struct.HasNativeTypes)
			{
				StringBuilder sb = new StringBuilder("( ");

				foreach (NativeType nt in sti.Definition.Struct.NativeTypes)
				{
					sb.Append("(");
					sb.Append(nt.Name);
					sb.Append(", ");
					sb.Append(nt.GetValue(Program.SemanticTypeSystem, sti.Instance).SafeToString());
					sb.Append(") ");
				}

				sb.Append(")");
				row[1] = sb.ToString();
			}

			dt.Rows.Add(row);
		}
	}
}
