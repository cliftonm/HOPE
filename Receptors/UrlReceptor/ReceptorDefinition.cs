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
	public class ReceptorDefinition : IReceptorInstance
	{
		public string Name { get { return "URL"; } }
		public bool IsEdgeReceptor { get { return false; } }
		public bool IsHidden { get { return false; } }

		protected IReceptorSystem rsys;

		public ReceptorDefinition(IReceptorSystem rsys)
		{
			this.rsys = rsys;
		}

		public void Initialize()
		{
		}

		public void Terminate()
		{
		}

		public string[] GetReceiveProtocols()
		{
			return new string[] { "URL" };
		}

		public string[] GetEmittedProtocols()
		{
			return new string[] { };
		}

		public void ProcessCarrier(ICarrier carrier)
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
