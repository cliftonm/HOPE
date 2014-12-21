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
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Clifton.ExtensionMethods;
using Clifton.Tools.Strings.Extensions;

using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

namespace SearchForReceptor
{
	/// <summary>
	/// Displays an input form for the user to enter a search string.
	/// </summary>
	public class ReceptorDefinition : BaseReceptor
	{
		public override string Name { get { return "Search For"; } }
		protected TextBox tb;

		public ReceptorDefinition(IReceptorSystem rsys) : base(rsys)
		{
			AddEmitProtocol("SearchFor");
			CreateForm();
		}

		protected void CreateForm()
		{
			Form form = new Form();
			form.Text = "Search For:";
			form.Location = new Point(100, 100);
			form.Size = new Size(500, 60);
			tb = new TextBox();
			tb.KeyPress += OnKeyPress;
			form.Controls.Add(tb);
			tb.Dock = DockStyle.Fill;
			form.Show();
			form.FormClosed += WhenFormClosed;
		}

		protected void OnKeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == '\r')
			{
				ISemanticTypeStruct protocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("SearchFor");
				dynamic signal = rsys.SemanticTypeSystem.Create("SearchFor");
				signal.SearchString = tb.Text;
				rsys.CreateCarrier(this, protocol, signal);
			}
		}

		protected void WhenFormClosed(object sender, FormClosedEventArgs e)
		{
			// Remove ourselves when the form is closed.
			rsys.Remove(this);
		}
	}
}
