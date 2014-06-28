using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clifton.ExtensionMethods;
using Clifton.Tools.Strings.Extensions;

using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

namespace TabbedDataTableReceptor
{
	public class TabbedDataTable : BaseReceptor
    {
		// Always override the Name property.
		public override string Name { get { return "Tabbed DataTable"; } }

		// Other optional property overides...

		/// <summary>
		/// Constructor called when the class is instantiated.
		/// Do not make any assumptions about the existence of other receptors at this point.
		/// Typically, all we do here is register emitted and received protocols.
		/// </summary>
		public TabbedDataTable(IReceptorSystem rsys)
			: base(rsys)
		{
			// Protocols emitted by this receptor
			// AddEmitProtocol("[protocol name]");

			// Protocols received by this receptor
			 AddReceiveProtocol("[protocol name]",
				 // Qualifier:
				 signal =>
				 {
					 // Put any qualifiers here to filter specific signals based on signal data.
					 // Replace the "return true" with a bool result.
					 return true;
				 },
				 // Action:
				signal =>
				{
					// Put any actions, method calls, etc., when the qualified signal is received.
				});
		}

		/// <summary>
		/// Any custom initialization after instantiation goes here.
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();
		}

		/// <summary>
		/// Any custom initialization after all receptors in the applet have been loaded goes here.
		/// </summary>
		public override void EndSystemInit()
		{
			base.EndSystemInit();
		}
    }
}
