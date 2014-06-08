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
