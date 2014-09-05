// #define TEST

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

		public Alchemy(IReceptorSystem rsys)
			: base(rsys)
		{
			AddEmitProtocol("AlchemyEntity");
			AddEmitProtocol("AlchemyKeyword");
			AddEmitProtocol("AlchemyConcept");
			AddEmitProtocol("Exception");

			// TODO: Temporary, for demo purposes only
			AddEmitProtocol("RequireTable");

			AddReceiveProtocol("URL",
				// cast is required to resolve Func vs. Action in parameter list.
				(Action<dynamic>)(signal => ParseUrl(signal)));

		}

		public override void Initialize()
		{
			base.Initialize();
			InitializeAlchemy();
		}

		protected void InitializeAlchemy()
		{
			alchemyObj = new AlchemyAPI.AlchemyAPI();
			alchemyObj.LoadAPIKey("alchemyapikey.txt");
		}

		/// <summary>
		/// Calls the AlchemyAPI to parse the URL.  The results are 
		/// emitted to an NLP Viewer receptor and to the database for
		/// later querying.
		/// </summary>
		/// <param name="signal"></param>
		protected async void ParseUrl(dynamic signal)
		{
			string url = signal.Value;
			DataSet dsEntities = null;
			DataSet dsKeywords = null;
			DataSet dsConcepts = null;

			// Exceptions need to be handled in the application thread.
			try
			{
				dsEntities = await Task.Run(() => { return GetEntities(url); });
				dsKeywords = await Task.Run(() => { return GetKeywords(url); });
				dsConcepts = await Task.Run(() => { return GetConcepts(url); });
			}
			catch (Exception ex)
			{
				EmitException(ex);
				return;
			}

			dsEntities.Tables["entity"].IfNotNull(t => Emit("AlchemyEntity", t, url));
			dsKeywords.Tables["keyword"].IfNotNull(t => Emit("AlchemyKeyword", t, url));
			dsConcepts.Tables["concept"].IfNotNull(t => Emit("AlchemyConcept", t, url));
		}

		protected void Emit(string protocol, DataTable data, string url)
		{
			data.ForEach(row =>
				{
					CreateCarrierIfReceiver(protocol, signal =>
						{
							signal.URL.Value = url;	// .Value because this is a semantic element and Value drills into the implementing native type.
							// Use the protocol as the driver of the fields we want to emit.
							ISemanticTypeStruct st = rsys.SemanticTypeSystem.GetSemanticTypeStruct(protocol);

							st.AllTypes.ForEach(se =>
								{
									// Sometimes a column will be missing.
									if (data.Columns.Contains(se.Name))
									{
										object val = row[se.Name];

										if (val != null && val != DBNull.Value)
										{
											se.SetValue(rsys.SemanticTypeSystem, signal, val);
										}
									}
								});
						});
				});
		}

		protected DataSet GetEntities(string url)
		{
			DataSet dsEntities = new DataSet();
#if TEST
			// Using previously captured dataset
			dsEntities.ReadXml("alchemyEntityTestResponse.xml");
#else
			if (!Cached("Entity", url, ref dsEntities))
			{
				AlchemyAPI_EntityParams eparams = new AlchemyAPI_EntityParams();
				eparams.setMaxRetrieve(250);
				string xml = alchemyObj.URLGetRankedNamedEntities(url, eparams);
				TextReader tr = new StringReader(xml);
				XmlReader xr = XmlReader.Create(tr);
				dsEntities.ReadXml(xr);
				xr.Close();
				tr.Close();
				Cache("Entity", url, dsEntities);
			}
#endif
			return dsEntities;
		}

		protected DataSet GetKeywords(string url)
		{
			DataSet dsKeywords = new DataSet();

#if TEST
			// Using previously captured dataset
			dsKeywords.ReadXml("alchemyKeywordsTestResponse.xml");
#else
			if (!Cached("Keyword", url, ref dsKeywords))
			{
				AlchemyAPI_KeywordParams eparams = new AlchemyAPI_KeywordParams();
				eparams.setMaxRetrieve(250);
				string xml = alchemyObj.URLGetRankedKeywords(url);
				TextReader tr = new StringReader(xml);
				XmlReader xr = XmlReader.Create(tr);
				dsKeywords.ReadXml(xr);
				xr.Close();
				tr.Close();
				Cache("Keyword", url, dsKeywords);
			}
#endif
			return dsKeywords;
		}

		protected DataSet GetConcepts(string url)
		{
			DataSet dsConcepts = new DataSet();

#if TEST
			// Using previously captured dataset
			dsConcepts.ReadXml("alchemyConceptsTestResponse.xml");
#else
			if (!Cached("Concept", url, ref dsConcepts))
			{
				AlchemyAPI_ConceptParams eparams = new AlchemyAPI_ConceptParams();
				eparams.setMaxRetrieve(250);
				string xml = alchemyObj.URLGetRankedConcepts(url, eparams);
				TextReader tr = new StringReader(xml);
				XmlReader xr = XmlReader.Create(tr);
				dsConcepts.ReadXml(xr);
				xr.Close();
				tr.Close();
				Cache("Concept", url, dsConcepts);
			}
#endif
			return dsConcepts;
		}

		/// <summary>
		/// Return true if cached and populate the refenced DataSet parameter.
		/// </summary>
		protected bool Cached(string prefix, string url, ref DataSet ds)
		{
			string urlHash = url.GetHashCode().ToString();
			string fn = prefix + "-" + urlHash + ".xml";

			bool cached = File.Exists(fn);

			if (cached)
			{
				ds.ReadXml(fn);
			}

			return cached;
		}

		/// <summary>
		/// Cache the dataset.
		/// </summary>
		protected void Cache(string prefix, string url, DataSet ds)
		{
			string urlHash = url.GetHashCode().ToString();
			string fn = prefix + "-" + urlHash + ".xml";
			ds.WriteXml(fn);
		}
	}
}
