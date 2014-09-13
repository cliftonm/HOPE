using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

using TypeSystemExplorer.Controllers;
using TypeSystemExplorer.Models;
using TypeSystemExplorer.Views;

using Clifton.ApplicationStateManagement;
using Clifton.ExtensionMethods;
using Clifton.Receptor.Interfaces;
using Clifton.Tools.Data;
using Clifton.Tools.Strings.Extensions;
using Clifton.Windows.Forms.XmlTree;

using XTreeController;
using XTreeInterfaces;

namespace TypeSystemExplorer.Controllers
{
	public class ReceptorEntry
	{
		public string Name { get; set; }
		public string Filename { get; set; }

		public override string ToString()
		{
			return Name;
		}
	}

	public class ReceptorChooserController : ViewController<ReceptorChooserView>
	{
		protected List<ReceptorEntry> receptors;

		public ReceptorChooserController()
		{
			InitializeReceptorEntriesList();
		}

		public override void EndInit()
		{
			base.EndInit();
			InitializeReceptorListView();
		}

		// For each selected item, add the receptor onto the surface.
		protected void OnAddReceptors(object sender, EventArgs args)
		{
			IMembrane dropInto = Program.Skin;
			int x = 100;
			List<IReceptor> receptors = new List<IReceptor>();
			ApplicationController.VisualizerController.View.StartDrop = true;

			foreach (ReceptorEntry r in View.ReceptorList.CheckedItems)
			{
				ApplicationController.VisualizerController.View.ClientDropPoint = ApplicationController.VisualizerController.View.NegativeSurfaceOffsetAdjust(new Point(x, 100));
				IReceptor droppedReceptor = dropInto.RegisterReceptor(Path.GetFullPath(r.Filename));
				receptors.Add(droppedReceptor);
				dropInto.LoadReceptors();
				x += 80;
			}

			receptors.ForEach(r => r.Instance.EndSystemInit());
			ApplicationController.VisualizerController.View.StartDrop = false;

			View.ReceptorList.UncheckAllItems();
		}

		protected void OnClear(object sender, EventArgs args)
		{
			View.ReceptorList.UncheckAllItems();
		}

		protected void InitializeReceptorEntriesList()
		{
			receptors = new List<ReceptorEntry>();
			receptors.Add(new ReceptorEntry() { Name = "Hello World", Filename = "HelloWorldReceptor.dll" });
			receptors.Add(new ReceptorEntry() { Name = "Logger", Filename = "LoggingReceptor.dll" });
			receptors.Add(new ReceptorEntry() { Name = "Text Display", Filename = "TextDisplayReceptor.dll" });
			receptors.Add(new ReceptorEntry() { Name = "Text To Speech", Filename = "TextToSpeechReceptor.dll" });
			receptors.Add(new ReceptorEntry() { Name = "Image Viewer", Filename = "ImageViewerReceptor.dll" });
			receptors.Add(new ReceptorEntry() { Name = "Carrier List Viewer", Filename = "CarrierListViewerReceptor.dll" });		// TODO: "Carrier" or "Signal" ?  What's the difference ?
			receptors.Add(new ReceptorEntry() { Name = "Tabbed Carrier List Viewer", Filename = "CarrierTabbedListViewerReceptor.dll" });		// TODO: "Carrier" or "Signal" ?  What's the difference ?

			receptors.Sort((r1, r2) => r1.Name.CompareTo(r2.Name));
		}

		protected void InitializeReceptorListView()
		{
			View.ReceptorList.DataSource = receptors;
			View.ReceptorList.DisplayMember = "Name";
			View.ReceptorList.ValueMember = "Filename";
		}
	}
}
