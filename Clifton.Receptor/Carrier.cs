using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

namespace Clifton.Receptor
{
	public class Carrier : ICarrier
	{
		public ISemanticTypeStruct Protocol { get; set; }
		public dynamic Signal { get; set; }

		public Carrier(ISemanticTypeStruct protocol, dynamic signal)
		{
			Protocol = protocol;
			Signal = signal;
		}
	}
}
