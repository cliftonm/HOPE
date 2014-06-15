using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;
using Clifton.Tools.Strings.Extensions;

namespace UrlReceptor
{
	public class ReceptorDefinition : BaseReceptor
	{
		public override string Name { get { return "URL"; } }
		
		public ReceptorDefinition(IReceptorSystem rsys) : base(rsys)
		{
			AddReceiveProtocol("URL");
		}

		public override void ProcessCarrier(ICarrier carrier)
		{
			string url = carrier.Signal.Value;
			try
			{
				Process.Start(url);
			}
			catch
			{
				// Eat exceptions.
			}
		}
	}
}
