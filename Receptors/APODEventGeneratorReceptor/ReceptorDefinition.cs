using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

namespace APODEventGeneratorReceptor
{
	public class ReceptorDefinition : BaseReceptor
	{
		public override string Name { get { return "APOD Date Generator"; } }
		
		public ReceptorDefinition(IReceptorSystem rsys) : base(rsys)
		{
			AddEmitProtocol("ScrapeAPOD");
		}

		public override void Initialize()
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
	}
}
