using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
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
	internal class MouseWheelMessageFilter : IMessageFilter
	{
		// P/Invoke declarations
		[DllImport("user32.dll")]
		private static extern IntPtr WindowFromPoint(Point pt);
		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

		const int WM_MOUSEWHEEL = 0x20a;

		public bool PreFilterMessage(ref Message m)
		{
			if (m.Msg == WM_MOUSEWHEEL)
			{
				// LParam contains the location of the mouse pointer
				Point pos = new Point(m.LParam.ToInt32() & 0xffff, m.LParam.ToInt32() >> 16);
				IntPtr hWnd = WindowFromPoint(pos);
				if (hWnd != IntPtr.Zero && hWnd != m.HWnd && Control.FromHandle(hWnd) != null)
				{
					// redirect the message to the correct control
					SendMessage(hWnd, m.Msg, m.WParam, m.LParam);
					return true;
				}
			}
			return false;
		}
	}


	// Register a "drop" receptor used exclusively for dropping messages onto a membrane.
	// This allows us to have a separate receptor from "System" and therefore (TODO: therefore what???)
	public class DropReceptor : BaseReceptor
	{
		public override string Name { get { return "DropReceptor"; } }
		public override bool IsHidden { get { return true; } }

		public DropReceptor(IReceptorSystem rsys)
			: base(rsys)
		{
		}
	}

	static class Program
	{
		public static Form MainForm;
		public static StatePersistence AppState;

		// TODO: Make this a protected property at some point?
		public static STS SemanticTypeSystem;

		// public static ReceptorsContainer Receptors;
		// The outermost membrane is called the "skin."

		public static Membrane Skin;
		public static DropReceptor DropReceptor;

		// TODO: Eventually put this somewhere.
		public static Dictionary<IReceptor, List<IReceptorConnection>> MasterReceptorConnectionList = new Dictionary<IReceptor, List<IReceptorConnection>>();

		[STAThread]
		static void Main()
		{
			Application.AddMessageFilter(new MouseWheelMessageFilter());

			try
			{
				SemanticTypeSystem = new STS();
				Skin = new Membrane(SemanticTypeSystem, null);
				Skin.Name = "Skin";
				// Receptors = new ReceptorsContainer();
				// Receptors.SemanticTypeSystem = SemanticTypeSystem;

				DropReceptor = new DropReceptor(Skin.ReceptorSystem);
				// Program.Skin.RegisterReceptor("DropReceptor", dr);

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
				System.Diagnostics.Debug.WriteLine(ex.Message);
				System.Diagnostics.Debugger.Break();
			}
		}
	}
}
