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

		/// <summary>
		/// The fully qualified path of this protocol, especially useful when recursing into child SE's when a carrier is emitted.
		/// </summary>
		public string ProtocolPath { get; set; }

		public Carrier(ISemanticTypeStruct protocol, string protocolPath, dynamic signal)
		{
			Protocol = protocol;
			Signal = signal;
			ProtocolPath = protocolPath;
		}
	}
}
