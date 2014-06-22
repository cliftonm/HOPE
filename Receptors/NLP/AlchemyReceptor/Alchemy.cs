using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AlchemyAPI;

using Clifton.ExtensionMethods;

using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

namespace AlchemyReceptor
{
    public class Alchemy : BaseReceptor
    {
		public override string Name { get { return "Alchemy"; } }
		public override bool IsEdgeReceptor { get { return true; } }

		protected AlchemyAPI.AlchemyAPI alchemyObj;

		public Alchemy(IReceptorSystem rsys)
			: base(rsys)
		{
			AddReceiveProtocol("URL",
				// cast is required to resolve Func vs. Action in parameter list.
				(Action<dynamic>)(signal => ParseUrl(signal)));
		}

		protected void ParseUrl(dynamic signal)
		{
			InitializeAlchemy();
			string url = signal.Value;
			GetEntities(url);
			GetKeywords(url);
			GetConcepts(url);
		}

		protected void InitializeAlchemy()
		{
			alchemyObj = new AlchemyAPI.AlchemyAPI();
			alchemyObj.LoadAPIKey("alchemyapikey.txt");
		}

		protected void GetEntities(string url)
		{
			string xml = alchemyObj.URLGetRankedNamedEntities(url);
		}

		protected void GetKeywords(string url)
		{
			string xml = alchemyObj.URLGetRankedKeywords(url);
		}

		protected void GetConcepts(string url)
		{
			string xml = alchemyObj.URLGetRankedConcepts(url);
		}
	}
}
