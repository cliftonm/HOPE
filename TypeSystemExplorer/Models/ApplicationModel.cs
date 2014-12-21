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
using System.Threading.Tasks;

using Clifton.ExtensionMethods;

namespace TypeSystemExplorer.Models
{
	public class ApplicationModel
	{
		protected bool isDirty;
		protected DataSet dataSet;
		protected string filename;
		protected string xmlFilename;

		public delegate void DirtyModelDlgt(bool dirtyState);
		public event DirtyModelDlgt ModelIsDirty;

		public bool HasFilename { get { return !String.IsNullOrEmpty(filename); } }
		public bool HasXmlFilename { get { return !String.IsNullOrEmpty(xmlFilename); } }

		public string Filename
		{
			get { return filename; }
			set { filename = value; }
		}

		public string XmlFilename
		{
			get { return xmlFilename; }
			set { xmlFilename = value; }
		}

		public bool IsDirty
		{
			get { return isDirty; }
			set
			{
				isDirty = value;
				ModelIsDirty.IfNotNull(e => e(isDirty));
			}
		}
/*
		public void NewModel()
		{
			dataSet = SchemaHelper.CreateSchema();
			filename = String.Empty;
			IsDirty = false;
		}

		public void LoadModel(string filename)
		{
			this.filename = filename;
			dataSet = SchemaHelper.CreateSchema();
			dataSet.ReadXml(filename, XmlReadMode.IgnoreSchema);
			IsDirty = false;
		}

		public void SaveModel()
		{
			dataSet.WriteXml(filename, XmlWriteMode.WriteSchema);
			IsDirty = false;
		}

		public void SaveModelAs(string filename)
		{
			this.filename = filename;
			dataSet.WriteXml(filename, XmlWriteMode.WriteSchema);
			IsDirty = false;
		}
 */ 
	}
}
