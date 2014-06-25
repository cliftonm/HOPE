using System;
using System.Collections.Generic;
using System.Data;
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
		protected DataSet dsEntities;

		protected Dictionary<string, int> resultTypeIDMap;
		protected Dictionary<string, int> entityTypeIDMap;

		public Alchemy(IReceptorSystem rsys)
			: base(rsys)
		{
			AddEmitProtocol("RequireTable");
			AddEmitProtocol("DatabaseProtocol");

			AddReceiveProtocol("URL",
				// cast is required to resolve Func vs. Action in parameter list.
				(Action<dynamic>)(signal => ParseUrl(signal)));

			AddReceiveProtocol("IDReturn", s => s.TableName == "AlchemyEntityType", s => entityTypeIDMap[s.Tag] = s.ID);
			AddReceiveProtocol("IDReturn", s => s.TableName == "AlchemyResultType", s => resultTypeIDMap[s.Tag] = s.ID);
		}

		public override void Initialize()
		{
			base.Initialize();

			// We need some database tables of we're going to persist and associate feeds and feed items with other data.
			RequireAlchemyTables();
			PopulateResultTypesIfMissing();
		}

		protected void RequireAlchemyTables()
		{
			RequireTable("AlchemyResult");
			RequireTable("AlchemyEntityType");
			RequireTable("AlchemyResultType");
		}

		/// <summary>
		/// Calls the AlchemyAPI to parse the URL.  The results are 
		/// emitted to an NLP Viewer receptor and to the database for
		/// later querying.
		/// </summary>
		/// <param name="signal"></param>
		protected void ParseUrl(dynamic signal)
		{
			InitializeAlchemy();
			string url = signal.Value;

			ParseEntities(url);
			ParseKeywords(url);

			// Extract to function
			DataSet ret = GetConcepts(url);
		}

		/// <summary>
		/// Populates the unique entity types and then associates the entities for this URL with their types.
		/// </summary>
		/// <param name="url"></param>
		protected void ParseEntities(string url)
		{
			DataSet ret = GetEntities(url);
			PersistUniqueEntityTypes(ret.Tables["entity"]);
		}

		protected void ParseKeywords(string url)
		{
			DataSet ret = GetKeywords(url);
			PersistUniqueKeywords(ret.Tables["keyword"]);
		}

		protected void InitializeAlchemy()
		{
			alchemyObj = new AlchemyAPI.AlchemyAPI();
			alchemyObj.LoadAPIKey("alchemyapikey.txt");
		}

		protected DataSet GetEntities(string url)
		{
			dsEntities = new DataSet();
			// string xml = alchemyObj.URLGetRankedNamedEntities(url);

			// Temporary hardcoded test.
			dsEntities.ReadXml("alchemyEntityTestResponse.xml");

			return dsEntities;
		}

		protected DataSet GetKeywords(string url)
		{
			DataSet ds = new DataSet();
			// string xml = alchemyObj.URLGetRankedKeywords(url);

			// Temporary hardcoded test.
			ds.ReadXml("alchemyKeywordsTestResponse.xml");

			return ds;
		}

		protected DataSet GetConcepts(string url)
		{
			DataSet ds = new DataSet();
			// string xml = alchemyObj.URLGetRankedConcepts(url);

			// Temporary hardcoded test.
			ds.ReadXml("alchemyConceptsTestResponse.xml");

			return ds;
		}

		protected void PopulateResultTypesIfMissing()
		{
			resultTypeIDMap = new Dictionary<string, int>();

			(new string[] { "Keyword", "Entity", "Concept" }).ForEach(t =>
			{
				CreateCarrierIfReceiver("DatabaseRecord", signal =>
				{
					signal.TableName = "AlchemyResultType";
					signal.Action = "InsertIfMissing";
					signal.Row = InstantiateCarrier("AlchemyResultType", rowSignal => rowSignal.Name = t);
					signal.UniqueKey = "Name";
					signal.Tag = t;
				});
			});
		}

		/// <summary>
		///  Gather all the entity types and create new entries if the entity type does not exist in the database.
		/// </summary>
		protected void PersistUniqueEntityTypes(DataTable dtEntity)
		{
			entityTypeIDMap = new Dictionary<string, int>();
			List<string> typeNames = new List<string>();
			dtEntity.ForEach(row => typeNames.Add(row["type"].ToString()));
			typeNames.Distinct().ForEach(t =>
				{
					CreateCarrierIfReceiver("DatabaseRecord", signal =>
					{
						signal.TableName = "AlchemyEntityType";
						signal.Action = "InsertIfMissing";
						signal.Row = InstantiateCarrier("AlchemyEntityType", rowSignal => rowSignal.Name = t);
						signal.UniqueKey = "Name";
						signal.Tag = t;
					});
				});
		}

		protected void PersistUniqueKeywords(DataTable dtKeyword)
		{
		}

		protected void PersistEntities(Dictionary<string, int> entityTypeIDMap, DataSet dsEntities)
		{
		}
	}
}
