using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;				// So we can get a timer marshalled on the UI thread.

using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

namespace TimerReceptor
{
	internal class IntervalTimer
	{
		public DateTime Start { get; set; }
		public decimal Interval { get; set; }
		public string EventName { get; set; }
		public Timer Timer { get; set; }

		// Internal, to track last time event was fired.
		public DateTime LastEventTime { get; set; }
	}

	public class ReceptorDefinition : IReceptorInstance
	{
		public string Name { get { return "Interval Timer"; } }
		public bool IsEdgeReceptor { get { return false; } }
		public bool IsHidden { get { return false; } }

		protected IReceptorSystem rsys;

		private List<IntervalTimer> intervals;

		public ReceptorDefinition(IReceptorSystem rsys)
		{
			this.rsys = rsys;
			intervals = new List<IntervalTimer>();
		}

		public string[] GetReceiveProtocols()
		{
			return new string[] { "IntervalTimerConfiguration" };
		}

		public void Terminate()
		{
		}

		public void ProcessCarrier(ICarrier carrier)
		{
			IntervalTimer interval = new IntervalTimer() { Start = carrier.Signal.StartDateTime, Interval = carrier.Signal.Interval, EventName = carrier.Signal.EventName };
			intervals.Add(interval);
		}
	}
}


