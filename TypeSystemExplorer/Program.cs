using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

using Clifton.ApplicationStateManagement;
using Clifton.MycroParser;
using Clifton.Tools.Strings.Extensions;

using Clifton.Receptor;
using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem;

namespace TypeSystemExplorer
{
	static class Program
	{
		public static Form MainForm;
		public static StatePersistence AppState;

		// TODO: Make this a protected property at some point?
		public static STS SemanticTypeSystem;

		// public static ReceptorsContainer Receptors;
		// The outermost membrane is called the "skin."

		public static Membrane Skin;

		// TODO: Eventually put this somewhere.
		public static Dictionary<IReceptor, List<IReceptor>> MasterReceptorConnectionList = new Dictionary<IReceptor, List<IReceptor>>();

		[STAThread]
		static void Main()
		{
			try
			{
				SemanticTypeSystem = new STS();
				Skin = new Membrane(SemanticTypeSystem);
				Skin.Name = "Skin";
				// Receptors = new ReceptorsContainer();
				// Receptors.SemanticTypeSystem = SemanticTypeSystem;

				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);

				AppState = new StatePersistence();
				AppState.ReadState("appState.xml");																	// Load the last application state.
				MainForm = MycroParser.InstantiateFromFile<Form>("mainform.xml", null);
				Application.Run(MainForm);
				AppState.WriteState("appState.xml");																// Save the application state.
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debugger.Break();
			}
		}
	}
}
