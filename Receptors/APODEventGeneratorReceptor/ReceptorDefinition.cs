using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

namespace APODEventGeneratorReceptor
{
	public class ReceptorDefinition : IReceptorInstance
	{
		public string Name { get { return "APOD Date Generator"; } }
		public bool IsEdgeReceptor { get { return false; } }
		public bool IsHidden { get { return false; } }

		protected IReceptorSystem rsys;

		public ReceptorDefinition(IReceptorSystem rsys)
		{
			this.rsys = rsys;
		}

		public void Initialize()
		{
			DateTime start = DateTime.Parse("6/16/1995");
			// DateTime stop = start.AddMonths(1);
			DateTime stop = DateTime.Now;  

			for (DateTime date = start; date <= stop; date = date.AddDays(1))
			{
				ISemanticTypeStruct protocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("TimerEvent");
				dynamic signal = rsys.SemanticTypeSystem.Create("TimerEvent");
				signal.EventName = "ScrapeAPOD";
				signal.EventDateTime = date;
				rsys.CreateCarrier(this, protocol, signal);
			}

			rsys.Remove(this);
		}

		public void Terminate()
		{
		}

		public string[] GetReceiveProtocols()
		{
			return new string[] { };
		}

		public string[] GetEmittedProtocols()
		{
			return new string[] { "ScrapeAPOD" };
		}

		public void ProcessCarrier(ICarrier carrier)
		{
		}
	}
}
