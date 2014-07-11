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
			
			DataSet dsEntities = await Task.Run(() => { return GetEntities(url); });
			DataSet dsKeywords = await Task.Run(() => { return GetKeywords(url); });
			DataSet dsConcepts = await Task.Run(() => { return GetConcepts(url); });

			dsEntities.Tables["entity"].IfNotNull(t => Emit("AlchemyEntity", t));
			dsKeywords.Tables["keyword"].IfNotNull(t => Emit("AlchemyKeyword", t));
			dsConcepts.Tables["concept"].IfNotNull(t => Emit("AlchemyConcept", t));
		}

		protected void Emit(string protocol, DataTable data)
		{
			data.ForEach(row =>
				{
					CreateCarrierIfReceiver(protocol, signal =>
						{
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
			try
			{
				AlchemyAPI_EntityParams eparams = new AlchemyAPI_EntityParams();
				eparams.setMaxRetrieve(250);
				string xml = alchemyObj.URLGetRankedNamedEntities(url, eparams);
				TextReader tr = new StringReader(xml);
				XmlReader xr = XmlReader.Create(tr);
				dsEntities.ReadXml(xr);
				xr.Close();
				tr.Close();
			}
			catch(Exception ex)
			{
				EmitException("Alchemy Receptor", ex);
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
			try
			{
				AlchemyAPI_KeywordParams eparams = new AlchemyAPI_KeywordParams();
				eparams.setMaxRetrieve(250);
				string xml = alchemyObj.URLGetRankedKeywords(url);
				TextReader tr = new StringReader(xml);
				XmlReader xr = XmlReader.Create(tr);
				dsKeywords.ReadXml(xr);
				xr.Close();
				tr.Close();
			}
			catch(Exception ex)
			{
				EmitException("Alchemy Receptor", ex);
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
			try
			{
				AlchemyAPI_ConceptParams eparams = new AlchemyAPI_ConceptParams();
				eparams.setMaxRetrieve(250);
				string xml = alchemyObj.URLGetRankedConcepts(url);
				TextReader tr = new StringReader(xml);
				XmlReader xr = XmlReader.Create(tr);
				dsConcepts.ReadXml(xr);
				xr.Close();
				tr.Close();
			}
			catch(Exception ex)
			{
				EmitException("Alchemy Receptor", ex);
			}
#endif
			return dsConcepts;
		}
	}
}
