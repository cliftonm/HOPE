/*
    Copyright 2104 Higher Order Programming

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

*/

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

		public ICarrier ParentCarrier { get; set; }

		public Carrier(ISemanticTypeStruct protocol, string protocolPath, dynamic signal, ICarrier parentCarrier)
		{
			Protocol = protocol;
			Signal = signal;
			ProtocolPath = protocolPath;
			ParentCarrier = parentCarrier;
		}
	}
}
