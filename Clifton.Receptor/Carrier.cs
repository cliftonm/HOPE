using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

namespace Clifton.Receptor
{
	/// <summary>
	/// A carrier is simply a container for a signal that complies with the specified protocol.
	/// </summary>
	public class Carrier : ICarrier
	{
		/// <summary>
		/// The protocol is a semantic type.
		/// </summary>
		public ISemanticTypeStruct Protocol { get; set; }

		/// <summary>
		/// The signal is a dynamically created object, created from the protocol's definition.
		/// </summary>
		public dynamic Signal { get; set; }

		public Carrier(ISemanticTypeStruct protocol, dynamic signal)
		{
			Protocol = protocol;
			Signal = signal;
		}
	}
}
