using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clifton.Receptor.Interfaces;

namespace Clifton.Receptor
{
	public class ReceptorConnection : IReceptorConnection
	{
		public IReceptor Receptor { get; set; }
		public bool RootOnly { get; set; }

		public ReceptorConnection(IReceptor receptor)
		{
			Receptor = receptor;
		}

		public ReceptorConnection(IReceptor receptor, bool rootOnly)
		{
			Receptor = receptor;
			RootOnly = rootOnly;
		}
	}
}
