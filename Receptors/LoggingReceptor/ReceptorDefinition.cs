using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

namespace LoggerReceptor
{
	/// <summary>
	/// This receptor is an edge receptor, receiving DebugMessage carriers and outputting them to a logging window.
	/// </summary>
	public class ReceptorDefinition : BaseReceptor
	{
		public override string Name { get { return "Logger"; } }
		public override bool IsEdgeReceptor { get { return true; } }

		public ReceptorDefinition(IReceptorSystem rsys) : base(rsys)
		{
			AddReceiveProtocol("DebugMessage");
			AddEmitProtocol("SystemMessage");
		}

		public override void ProcessCarrier(ICarrier carrier)
		{
			if (carrier.Protocol.DeclTypeName == "DebugMessage")
			{
				string msg = carrier.Signal.Message;
				System.Diagnostics.Debug.WriteLine(msg);

				Flyout(msg);
			}
		}

		/// <summary>
		/// A visualization at the system level.
		/// </summary>
		/// <param name="msg"></param>
		protected void Flyout(string msg)
		{
			ISemanticTypeStruct protocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("SystemMessage");
			dynamic signal = rsys.SemanticTypeSystem.Create("SystemMessage");
			signal.Action = "Flyout";
			signal.Data = msg;
			signal.Source = this;
			rsys.CreateCarrier(this, protocol, signal);
		}
	}
}
