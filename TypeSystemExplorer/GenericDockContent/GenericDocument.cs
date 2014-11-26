using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WeifenLuo.WinFormsUI.Docking;

using Hope.Interfaces;

namespace TypeSystemExplorer
{
	public class GenericDocument : DockContent, IGenericDock, IGenericDocument
	{
		public string ContentMetadata { get; set; }

		public GenericDocument()
		{
			ContentMetadata = String.Empty;
		}

		public GenericDocument(string contentMetadata)
		{
			ContentMetadata = contentMetadata;
		}

		protected override string GetPersistString()
		{
			return GetType().ToString() + "," + ContentMetadata;
		}
	}
}
