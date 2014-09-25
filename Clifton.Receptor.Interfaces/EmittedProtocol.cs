using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clifton.Receptor.Interfaces
{
	public class EmittedProtocol
	{
		protected bool enabled;

		public string Protocol { get; set; }
		public bool Enabled
		{
			get { return enabled; }
			set { enabled = value; }
		}

		public EmittedProtocol()
		{
			enabled = true;
		}
	}
}
