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
#pragma warning disable 67
		public event EventHandler<EventArgs> ReceiveProtocolsChanged;
		public event EventHandler<EventArgs> EmitProtocolsChanged;
#pragma warning restore 67

		public string Name { get { return "Heartbeat"; } }
		public bool IsEdgeReceptor { get { return false; } }
		public bool IsHidden { get { return false; } }

		public IReceptorSystem ReceptorSystem
		{
			get { return rsys; }
			set { rsys = value; }
		}

		protected IReceptorSystem rsys;
		protected Timer timer;

		public ReceptorDefinition(IReceptorSystem rsys)
		{
			this.rsys = rsys;
			InitializeRepeatedHelloEvent();
		}

		public void Initialize()
		{
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

		public string[] GetEmittedProtocols()
		{
			return new string[] { "DebugMessage" };
		}

		public void ProcessCarrier(ICarrier carrier)
		{
		}

		protected void InitializeRepeatedHelloEvent()
		{
			timer = new Timer();
			timer.Interval = 1000 * 2;		// every 2 seconds.
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
