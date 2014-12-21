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

namespace Clifton.Receptor.Interfaces
{
	/// <summary>
	/// Event args for new membrane notification.
	/// </summary>
	public class MembraneEventArgs : EventArgs
	{
		public IMembrane Membrane { get; protected set; }

		public MembraneEventArgs(IMembrane m)
		{
			Membrane = m;
		}
	}

	/// <summary>
	/// Event args for a new receptor notification.
	/// </summary>
	public class ReceptorEventArgs : EventArgs
	{
		public IReceptor Receptor { get; protected set; }

		public ReceptorEventArgs(IReceptor receptor)
		{
			Receptor = receptor;
		}
	}

	/// <summary>
	/// Event args for a new carrier notification.
	/// </summary>
	public class NewCarrierEventArgs : EventArgs
	{
		public IReceptorInstance From { get; protected set; }
		public ICarrier Carrier { get; protected set; }

		public NewCarrierEventArgs(IReceptorInstance from, ICarrier carrier)
		{
			From = from;
			Carrier = carrier;
		}
	}

}
