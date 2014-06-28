using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clifton.ExtensionMethods;
using Clifton.Tools.Strings.Extensions;

using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

// 1. Copy this project...
//		Properties folder
//		ReceptorTemplate.csproj
//		YourReceptor.cs
// ... to the desired folder.
// 2. Rename the .csproj and .cs files to the desired names.
// 3. Add it as an existing project to your own solution.
// 4. Remove YourReceptor.cs
// 4. Add the renamed (see step 2) .cs file
// 5. Add a post-build event to copy the receptor to your exe:
//		for example: copy TabbedDataTableReceptor.dll ..\..\..\..\TypeSystemExplorer\bin\Debug
//		TODO: We should resolve receptor locations using a config file of paths
// 6. Change the namespace, class name, and Name property get return value.

// TODO: All of this should just be auto-gen'd by something.

namespace ReceptorTemplate
{
	public class YourReceptor : BaseReceptor
    {
		// Always override the Name property.
		public override string Name { get { return "Your Receptor"; } }

		// Other optional property overides...

		/// <summary>
		/// Constructor called when the class is instantiated.
		/// Do not make any assumptions about the existence of other receptors at this point.
		/// Typically, all we do here is register emitted and received protocols.
		/// </summary>
		public YourReceptor(IReceptorSystem rsys)
			: base(rsys)
		{
			// Protocols emitted by this receptor
			 AddEmitProtocol("[protocol name]");

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
