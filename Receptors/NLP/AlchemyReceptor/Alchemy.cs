using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

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
		protected string url;				// The URL we are currently processing.
		protected DataSet dsEntities;
		protected DataSet dsKeywords;
		protected DataSet dsConcepts;
		protected int feedItemID;
		protected int alchemyResultID;
		protected DateTime captureDate;

		protected Dictionary<string, int> resultTypeIDMap;
		protected Dictionary<string, int> entityTypeIDMap;
		protected Dictionary<string, int> entityPhraseIDMap;
		protected Dictionary<string, int> keywordPhraseIDMap;
		protected Dictionary<string, int> conceptPhraseIDMap;
		protected Queue<string> urlQueue;

		protected const string EntitiesGate = "entities";
		protected const string KeywordsGate = "keywords";
		protected const string ConceptsGate = "concepts";
		protected const string CheckNlpGate = "checknlp";
		protected const string ProcessingUrlGate = "procurl";
		protected const string ProcessECK = "processECK";			// counts 3 -- entities, keywords, concepts, and triggers when all three are fully processed.

		protected const string FeedItemGate = "entityfeedItemid";

		// gates for entities
		protected const string EntityGate = "entityid";
		protected const string EntityPhraseGate = "entityphraseid";

		// gates for keywords
		protected const string keywordPhraseGate = "keywordphraseid";

		// gates for concepts
		protected const string ConceptPhraseGate = "conceptphraseid";

		public Alchemy(IReceptorSystem rsys)
			: base(rsys)
		{
			urlQueue = new Queue<string>();

			AddEmitProtocol("RequireTable");
			AddEmitProtocol("DatabaseRecord");

			AddReceiveProtocol("URL",
				// cast is required to resolve Func vs. Action in parameter list.
				(Action<dynamic>)(signal => ParseUrl(signal)));

			AddReceiveProtocol("IDReturn", s => s.TableName == "AlchemyResultType", s => resultTypeIDMap[s.Tag] = s.ID);

			AddReceiveProtocol("IDReturn", s => s.TableName == "AlchemyEntityType", s => 
				{
					entityTypeIDMap[s.Tag] = s.ID;
					DecrementCompositeGate(EntitiesGate, EntityGate);
				});

			AddReceiveProtocol("IDReturn", s => s.TableName == "AlchemyPhrase", s =>
			{
				char tagType = s.Tag[0];
				string tag = s.Tag.Substring(2);

				// TODO: Rather kludgy, encoding additional information in the tag.
				switch (tagType)
				{
					case 'E':
						{
							entityPhraseIDMap[tag] = s.ID;
							DecrementCompositeGate(EntitiesGate, EntityPhraseGate);
							break;
						}
					case 'C':
						{
							conceptPhraseIDMap[tag] = s.ID;
							DecrementGate(ConceptsGate);
							break;
						}
					case 'K':
						{
							keywordPhraseIDMap[tag] = s.ID;
							DecrementGate(KeywordsGate);
							break;
						}
				}
			});

			AddReceiveProtocol("IDReturn", s => s.TableName == "RSSFeedItem" && s.Tag == "Alchemy", s =>
				{
					feedItemID = s.ID;
					DecrementGate(FeedItemGate);
				});

			// We don't care about the AlchemyResult.ID's at the moment.
			AddReceiveProtocol("IDReturn", s => s.TableName == "AlchemyResult" && s.Tag == "Alchemy", s => {});

			AddReceiveProtocol("IDReturn", s => s.TableName == "AlchemyResult" && s.Tag == "AlchemyResultID", s =>
				{
					alchemyResultID = s.ID;
					DecrementGate(CheckNlpGate);
				});

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
			RequireTable("AlchemyEntityType");
			RequireTable("AlchemyResultType");
			RequireTable("AlchemyPhrase");
			// TODO: We should really create the FK's in the DB for validation purposes.
			RequireTable("AlchemyResult");
		}

		/// <summary>
		/// Calls the AlchemyAPI to parse the URL.  The results are 
		/// emitted to an NLP Viewer receptor and to the database for
		/// later querying.
		/// </summary>
		/// <param name="signal"></param>
		protected void ParseUrl(dynamic signal)
		{
			url = signal.Value;
			urlQueue.Enqueue(url);

			// If we're busy processing a URL, queue it...
			if (!gates.ContainsKey(ProcessingUrlGate) || gates[ProcessingUrlGate].Count == 0)
			{
				// ...otherwise, process it now and set up to queue others that come in.
				ProcessNextUrl();
			}
		}

		protected void ProcessNextUrl()
		{
			if (urlQueue.Count > 0)
			{
				url = urlQueue.Dequeue();
				captureDate = DateTime.Now;
				GetFeedItemID(url);					// We will be needing the ID of the URL.
				RegisterGate(FeedItemGate, 1, CheckIfParsed);
				RegisterGate(ProcessingUrlGate, 1, ProcessNextUrl);
			}
		}

		/// <summary>
		/// Checks if the url (feed item ID) has already been NLP'd.
		/// </summary>
		protected void CheckIfParsed()
		{
			GetNlpID(feedItemID);
			RegisterGate(CheckNlpGate, 1, GetDataFromAlchemy);
		}

		/// <summary>
		/// NLP's the url if we haven't NLP'd it already.  
		/// </summary>
		protected void GetDataFromAlchemy()
		{
			// We don't need to query Alchemy if we've already done so in the past.
			// TODO: Yes, I know, this assumes static content.
			if (alchemyResultID == -1)
			{
				InitializeAlchemy();
				RegisterGate(ProcessECK, 3, ProcessNextUrl);
				ParseEntities(url);
				ParseKeywords(url);
				ParseConcepts(url);
			}
			else
			{
				// Nothing to do.  Check the next queued URL.
				DecrementGate(ProcessingUrlGate);
			}
		}

		/// <summary>
		/// Populates the unique entity types and then associates the entities for this URL with their types.
		/// </summary>
		/// <param name="url"></param>
		protected async void ParseEntities(string url)
		{
			bool success = await Task<bool>.Run(() => { return GetEntities(url); });

			if (success)
			{
				if (dsEntities.Tables["entity"] != null)
				{
					PersistUniqueEntityTypes(dsEntities.Tables["entity"]);
					// Will continue with PersistEntities
				}
				else
				{
					DecrementGate(ProcessECK);
				}
			}
			else
			{
				DecrementGate(ProcessECK);
			}
		}

		protected async void ParseKeywords(string url)
		{
			bool success = await Task<bool>.Run(() => { return GetKeywords(url); });

			if (success)
			{
				if (dsKeywords.Tables["keyword"] != null)
				{
					PersistUniqueKeywords(dsKeywords.Tables["keyword"]);
					// Will continue with PersistKeywords
				}
				else
				{
					DecrementGate(ProcessECK);
				}
			}
			else
			{
				DecrementGate(ProcessECK);
			}
		}

		protected async void ParseConcepts(string url)
		{
			bool success = await Task<bool>.Run(() => { return GetConcepts(url); });

			if (success)
			{
				if (dsConcepts.Tables["concept"] != null)
				{
					PersistUniqueConcepts(dsConcepts.Tables["concept"]);
					// Will continue with PersistConcepts.
				}
				else
				{
					DecrementGate(ProcessECK);
				}
			}
			else
			{
				DecrementGate(ProcessECK);
			}
		}

		protected void InitializeAlchemy()
		{
			alchemyObj = new AlchemyAPI.AlchemyAPI();
			alchemyObj.LoadAPIKey("alchemyapikey.txt");
		}

		protected bool GetEntities(string url)
		{
			bool success = true;

			try
			{
				dsEntities = new DataSet();
				AlchemyAPI_EntityParams eparams = new AlchemyAPI_EntityParams();
				eparams.setSentiment(true);
				eparams.setMaxRetrieve(250);
				string xml = alchemyObj.URLGetRankedNamedEntities(url, eparams);
				TextReader tr = new StringReader(xml);
				XmlReader xr = XmlReader.Create(tr);
				dsEntities.ReadXml(xr);
				xr.Close();
				tr.Close();
			}
			catch
			{
				// TODO: Log errors.
				success = false;
			}

			return success;

			// Temporary hardcoded test.
			// dsEntities.ReadXml("alchemyEntityTestResponse.xml");
		}

		protected bool GetKeywords(string url)
		{
			bool success = true;

			try
			{
				dsKeywords = new DataSet();
				string xml = alchemyObj.URLGetRankedKeywords(url);
				TextReader tr = new StringReader(xml);
				XmlReader xr = XmlReader.Create(tr);
				dsKeywords.ReadXml(xr);
				xr.Close();
				tr.Close();
			}
			catch
			{
				// TODO: Log errors.
				success = false;
			}

			return success;

			// Temporary hardcoded test.
			// dsKeywords.ReadXml("alchemyKeywordsTestResponse.xml");
		}

		protected bool GetConcepts(string url)
		{
			bool success = true;

			try
			{
				dsConcepts = new DataSet();
				string xml = alchemyObj.URLGetRankedConcepts(url);
				TextReader tr = new StringReader(xml);
				XmlReader xr = XmlReader.Create(tr);
				dsConcepts.ReadXml(xr);
				xr.Close();
				tr.Close();
			}
			catch
			{
				// TODO: Log errors.
				success = false;
			}

			return success;

			// Temporary hardcoded test.
			// dsConcepts.ReadXml("alchemyConceptsTestResponse.xml");
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
			entityPhraseIDMap = new Dictionary<string, int>();

			List<string> typeNames = new List<string>();
			List<string> phrases = new List<string>();

			dtEntity.ForEach(row => typeNames.Add(row["type"].ToString()));
			dtEntity.ForEach(row => phrases.Add(row["text"].ToString()));

			var distinctTypes = typeNames.Distinct();
			var distinctPhrases = phrases.Distinct();

			// We need to wait for all the ID responses before continuing with the parsing of the entity fields.
			int distinctTypesCount = distinctTypes.Count();
			int distinctPhrasesCount = distinctPhrases.Count();

			// Getting to the next action requires that we have the phrase ID's and entity type ID's.
			RegisterCompositeGate(EntitiesGate, PersistEntities);
			RegisterCompositeGateGate(EntitiesGate, EntityGate, distinctTypesCount, NullAction);
			RegisterCompositeGateGate(EntitiesGate, EntityPhraseGate, distinctPhrasesCount, NullAction);
			
			distinctTypes.ForEach(t =>
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

			distinctPhrases.ForEach(t =>
				{
					CreateCarrierIfReceiver("DatabaseRecord", signal =>
					{
						signal.TableName = "AlchemyPhrase";
						signal.Action = "InsertIfMissing";
						signal.Row = InstantiateCarrier("AlchemyPhrase", rowSignal => rowSignal.Name = t);
						signal.UniqueKey = "Name";
						// TODO: This is rather kludgy.  Fix it.
						signal.Tag = "E:" + t;
					});
				});
		}

		protected void PersistUniqueKeywords(DataTable dtKeyword)
		{
			keywordPhraseIDMap = new Dictionary<string, int>();
			List<string> phrases = new List<string>();
			dtKeyword.ForEach(row => phrases.Add(row["text"].ToString()));
			var distinctPhrases = phrases.Distinct();
			int distinctPhrasesCount = distinctPhrases.Count();
			RegisterGate(KeywordsGate, distinctPhrasesCount, PersistKeywords);

			distinctPhrases.ForEach(t =>
			{
				CreateCarrierIfReceiver("DatabaseRecord", signal =>
				{
					signal.TableName = "AlchemyPhrase";
					signal.Action = "InsertIfMissing";
					signal.Row = InstantiateCarrier("AlchemyPhrase", rowSignal => rowSignal.Name = t);
					signal.UniqueKey = "Name";
					// TODO: This is rather kludgy.  Fix it.
					signal.Tag = "K:" + t;
				});
			});
		}

		protected void PersistUniqueConcepts(DataTable dtConcept)
		{
			conceptPhraseIDMap = new Dictionary<string, int>();
			List<string> phrases = new List<string>();
			dtConcept.ForEach(row => phrases.Add(row["text"].ToString()));
			var distinctPhrases = phrases.Distinct();
			int distinctPhrasesCount = distinctPhrases.Count();

			RegisterGate(ConceptsGate, distinctPhrasesCount, PersistConcepts);

			distinctPhrases.ForEach(t =>
			{
				CreateCarrierIfReceiver("DatabaseRecord", signal =>
				{
					signal.TableName = "AlchemyPhrase";
					signal.Action = "InsertIfMissing";
					signal.Row = InstantiateCarrier("AlchemyPhrase", rowSignal => rowSignal.Name = t);
					signal.UniqueKey = "Name";
					// TODO: This is rather kludgy.  Fix it.
					signal.Tag = "C:" + t;
				});
			});
		}

		/// <summary>
		/// Inserts entities only if the AlchemyResult does not contain the feed item ID.
		/// We assume that any instance of the feed item ID means that all entities have already been persisted.
		/// </summary>
		protected void PersistEntities()
		{
			dsEntities.Tables["entity"].ForEach(row =>
				{
					CreateCarrierIfReceiver("DatabaseRecord", signal =>
					{
						signal.TableName = "AlchemyResult";
						signal.Action = "InsertIfMissing";
						signal.Row = InstantiateCarrier("AlchemyResult", rowSignal =>
						{
							rowSignal.CaptureDate = captureDate;
							rowSignal.AlchemyPhraseID = entityPhraseIDMap[row["text"].ToString()];
							rowSignal.Relevance = Convert.ToDouble(row["relevance"]);
							rowSignal.RSSFeedItemID = feedItemID;
							rowSignal.AlchemyEntityTypeID = entityTypeIDMap[row["type"].ToString()];
							rowSignal.AlchemyResultTypeID = resultTypeIDMap["Entity"];
						});
						signal.UniqueKey = "RSSFeedItemID, AlchemyPhraseID, AlchemyEntityTypeID";			// composite UK.
						signal.Tag = "Alchemy";
					});
				});

			DecrementGate(ProcessECK);
		}

		protected void PersistKeywords()
		{
			dsKeywords.Tables["keyword"].ForEach(row =>
			{
				CreateCarrierIfReceiver("DatabaseRecord", signal =>
				{
					signal.TableName = "AlchemyResult";
					signal.Action = "InsertIfMissing";
					signal.Row = InstantiateCarrier("AlchemyResult", rowSignal =>
					{
						rowSignal.CaptureDate = captureDate;
						rowSignal.AlchemyPhraseID = keywordPhraseIDMap[row["text"].ToString()];
						rowSignal.Relevance = Convert.ToDouble(row["relevance"]);
						rowSignal.RSSFeedItemID = feedItemID;
						rowSignal.AlchemyResultTypeID = resultTypeIDMap["Keyword"];
					});
					signal.UniqueKey = "RSSFeedItemID, AlchemyPhraseID, AlchemyResultTypeID";			// composite UK.
					signal.Tag = "Alchemy";
				});
			});

			DecrementGate(ProcessECK);
		}

		protected void PersistConcepts()
		{
			dsConcepts.Tables["concept"].ForEach(row =>
			{
				CreateCarrierIfReceiver("DatabaseRecord", signal =>
				{
					signal.TableName = "AlchemyResult";
					signal.Action = "InsertIfMissing";
					signal.Row = InstantiateCarrier("AlchemyResult", rowSignal =>
					{
						// TODO: We are ignoring dbpedia, freebase, and opencyc at the moment.
						rowSignal.CaptureDate = captureDate;
						rowSignal.AlchemyPhraseID = conceptPhraseIDMap[row["text"].ToString()];
						rowSignal.Relevance = Convert.ToDouble(row["relevance"]);
						rowSignal.RSSFeedItemID = feedItemID;
						rowSignal.AlchemyResultTypeID = resultTypeIDMap["Concept"];
					});
					signal.UniqueKey = "RSSFeedItemID, AlchemyPhraseID, AlchemyResultTypeID";			// composite UK.
					signal.Tag = "Alchemy";
				});
			});

			DecrementGate(ProcessECK);
		}

		protected void GetFeedItemID(string url)
		{
			CreateCarrierIfReceiver("DatabaseRecord", signal =>
			{
				signal.TableName = "RSSFeedItem";
				signal.Action = "GetID";
				signal.UniqueKey = "URL";
				signal.UniqueKeyValue = url;
				signal.Tag = "Alchemy";
			});
		}

		/// <summary>
		/// Checks if there is an NLP record for the feed item ID.
		/// </summary>
		protected void GetNlpID(int feedItemID)
		{
			CreateCarrierIfReceiver("DatabaseRecord", signal =>
			{
				signal.TableName = "AlchemyResult";
				signal.Action = "GetID";
				signal.UniqueKey = "RSSFeedItemID";
				signal.UniqueKeyValue = feedItemID.ToString();
				signal.Tag = "AlchemyResultID";
			});
		}
	}
}
