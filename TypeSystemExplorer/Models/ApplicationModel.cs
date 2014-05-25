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

		public delegate void DirtyModelDlgt(bool dirtyState);
		public event DirtyModelDlgt ModelIsDirty;

		public bool HasFilename { get { return !String.IsNullOrEmpty(filename); } }

		public string Filename
		{
			get { return filename; }
			set { filename = value; }
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
	}
}
