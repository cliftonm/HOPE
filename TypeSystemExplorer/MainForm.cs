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


