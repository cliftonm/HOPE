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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

using Clifton.Assertions;
using Clifton.ExtensionMethods;
using Clifton.SemanticTypeSystem;

namespace TypeSystemExplorer
{
	public partial class MainForm : Form
	{
		// protected STS sts = new STS();

		public MainForm()
		{
		}

/*
		/// <summary>
		/// Parse the provided XML and create the semantic types.
		/// </summary>
		private void btnBuildTypeDefinitions_Click(object sender, EventArgs e)
		{
			try
			{
				sts.Parse("XML");
			}
			catch (Exception ex)
			{
				MessageBox.Show(Assert.LastErrorMessage(ex.Message), "An error has occurred.", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Assert.ErrorMessage = null;
			}

			UpdateRegisteredTypes();
		}


		/// <summary>
		/// Generate the backing code assembly for the semantic types.
		/// </summary>
		private void btnGenerate_Click(object sender, EventArgs e)
		{
			
		}

		private void UpdateRegisteredTypes()
		{
//			lbRegisteredTypes.Items.Clear();
//			sts.SemanticTypes.Keys.OrderBy(t => t).ForEach(t => lbRegisteredTypes.Items.Add(t));
		}

		private void btnClear_Click(object sender, EventArgs e)
		{
			sts.SemanticTypes.Clear();
			UpdateRegisteredTypes();
		}
 */ 
	}
}


