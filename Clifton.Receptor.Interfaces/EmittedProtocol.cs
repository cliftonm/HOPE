using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clifton.Receptor.Interfaces
{
	public class EmittedProtocol
	{
		public string Protocol { get; set; }
		public bool Enabled { get; set; }

		public EmittedProtocol()
		{
			Enabled = true;
		}
	}
}
