using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

namespace HelloWorldReceptor
{
    public class ReceptorDefinition : IReceptorInstance
    {
		public string Name { get { return "Heartbeat"; } }
		public bool IsEdgeReceptor { get { return false; } }
		public bool IsHidden { get { return false; } }

		protected IReceptorSystem rsys;
		protected Timer timer;

		public ReceptorDefinition(IReceptorSystem rsys)
		{
			this.rsys = rsys;
			InitializeRepeatedHelloEvent();
		}

		public void Terminate()
		{
			timer.Stop();
			timer.Dispose();
		}

		public string[] GetReceiveProtocols()
		{
			return new string[] {};
		}

		public void ProcessCarrier(ISemanticTypeStruct protocol, dynamic signal)
		{
		}

		protected void InitializeRepeatedHelloEvent()
		{
			timer = new Timer();
			timer.Interval = 1000 * 2;		// every 10 seconds.
			timer.Tick += SayHello;
			timer.Start();
		}

		protected void SayHello(object sender, EventArgs args)
		{
			ISemanticTypeStruct protocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("DebugMessage");
			dynamic signal = rsys.SemanticTypeSystem.Create("DebugMessage");
			signal.Message = "Hello World!";
			rsys.CreateCarrier(this, protocol, signal);
		}
    }
}
