using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Clifton.ExtensionMethods;
using Clifton.MycroParser;
using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;
using Clifton.Tools.Strings.Extensions;

// TODO: This is hard-coded for the Query protocol at the moment.
// TODO: The UI should really be dynamic based on the native types defined in the protocol.

namespace SignalCreatorReceptor
{
	public class SignalCreator : BaseReceptor
	{
		public override string Name { get { return "Signal Creator"; } }
		public override bool IsEdgeReceptor { get { return true; } }
		public override string ConfigurationUI { get { return "SignalCreatorConfig.xml"; } }

		[UserConfigurableProperty("Protocol Name:")]
		public string ProtocolName { get; set; }

		[UserConfigurableProperty("Data")]
		public string Data { get; set; }

		public SignalCreator(IReceptorSystem rsys)
			: base(rsys)
		{
		}

		public override void EndSystemInit()
		{
			base.EndSystemInit();

			if (!String.IsNullOrEmpty(ProtocolName))
			{
				AddEmitProtocol(ProtocolName);
				// TODO: User should define whether the signal is emitted right away on startup.
				CreateCarrier(ProtocolName, signal => signal.QueryText = Data);
			}
		}

		public override bool UserConfigurationUpdated()
		{
			base.UserConfigurationUpdated();
			bool ret = rsys.SemanticTypeSystem.VerifyProtocolExists(ProtocolName);

			if (ret)
			{
				RemoveEmitProtocols(); 
				AddEmitProtocol(ProtocolName);
				CreateCarrier(ProtocolName, signal => signal.QueryText = Data);
			}
			else
			{
				ConfigurationError = "The semantic type '" + ProtocolName + "' is not defined.";
			}

			return ret;
		}
	}
}
