using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

using Clifton.ApplicationStateManagement;
using Clifton.Tools.Strings.Extensions;
using Clifton.MycroParser;
using Clifton.Receptor;
using Clifton.SemanticTypeSystem;

namespace TypeSystemExplorer
{
	static class Program
	{
		public static Form MainForm;
		public static StatePersistence AppState;
		public static STS SemanticTypeSystem;
		public static ReceptorsContainer Receptors;

		[STAThread]
		static void Main()
		{
			try
			{
				SemanticTypeSystem = new STS();
				Receptors = new ReceptorsContainer();
				Receptors.SemanticTypeSystem = SemanticTypeSystem;

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
