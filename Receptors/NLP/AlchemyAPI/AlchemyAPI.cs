using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;

namespace AlchemyAPI
{
    public class AlchemyAPI
    {
    	private string _apiKey;
    	private string _requestUri;

    	public AlchemyAPI()
    	{
    		_apiKey = "";
    		_requestUri = "http://access.alchemyapi.com/calls/";
    	}

    	public void SetAPIHost(string apiHost)
    	{
    		if (apiHost.Length < 2)
    		{
    			System.ApplicationException ex =
    				new System.ApplicationException("Error setting API host.");

    			throw ex;
    		}

    		_requestUri = "http://" + apiHost  +  ".alchemyapi.com/calls/";
    	}

    	public void SetAPIKey(string apiKey)
    	{
    		_apiKey = apiKey;

    		if (_apiKey.Length < 5)
    		{
    			System.ApplicationException ex =
    				new System.ApplicationException("Error setting API key.");

    			throw ex;
    		}
    	}

    	public void LoadAPIKey(string filename)
    	{
    		StreamReader reader;

    		reader = File.OpenText(filename);

    		string line = reader.ReadLine();

    		reader.Close();

    		_apiKey = line.Trim();

    		if (_apiKey.Length < 5)
    		{
    			System.ApplicationException ex =
    				new System.ApplicationException("Error loading API key.");

    			throw ex;
    		}
    	}

    	#region GetAuthor
    	public string URLGetAuthor(string url)
    	{
    		CheckURL(url);
    			
    		return URLGetAuthor(url, new AlchemyAPI_BaseParams());
    	}
    		
    	public string URLGetAuthor(string url, AlchemyAPI_BaseParams parameters)
    	{
    		CheckURL(url);
    		parameters.setUrl(url);
    			
    		return GET("URLGetAuthor", "url", parameters);
    	}			
    		
    	public string HTMLGetAuthor(string html,string url)
    	{
    		CheckHTML(html, url);
    			
    		return HTMLGetAuthor(html, url, new AlchemyAPI_BaseParams());
    	}
    		
    	public string HTMLGetAuthor(string html, string url, AlchemyAPI_BaseParams parameters)
    	{
    		CheckHTML(html, url);
    		parameters.setHtml(html);
    		parameters.setUrl(url);
    			
    		return POST("HTMLGetAuthor", "html", parameters);
    	}	
    	#endregion

    	#region GetRankedNamedEntities
    	public string URLGetRankedNamedEntities(string url)
    	{
    		CheckURL(url);

    		return URLGetRankedNamedEntities(url, new AlchemyAPI_EntityParams());
    	}

    	public string URLGetRankedNamedEntities(string url, AlchemyAPI_EntityParams parameters)
    	{
    		CheckURL(url);
    		parameters.setUrl(url);
    	
    		return GET("URLGetRankedNamedEntities", "url", parameters);
    	}

    	public string HTMLGetRankedNamedEntities(string html, string url)
    	{
    		CheckHTML(html, url);

    		return HTMLGetRankedNamedEntities(html, url, new AlchemyAPI_EntityParams());
    	}

    	
    	public string HTMLGetRankedNamedEntities(string html, string url, AlchemyAPI_EntityParams parameters)
    	{
    		CheckHTML(html, url);
    		parameters.setHtml(html);
    		parameters.setUrl(url);

    		return POST("HTMLGetRankedNamedEntities", "html", parameters);
    	}

    	public string TextGetRankedNamedEntities(string text)
    	{
    		CheckText(text);

    		return TextGetRankedNamedEntities(text,new AlchemyAPI_EntityParams());
    	}

    	public string TextGetRankedNamedEntities(string text, AlchemyAPI_EntityParams parameters)
    	{
    		CheckText(text);
    		parameters.setText(text);

    		return POST("TextGetRankedNamedEntities", "text", parameters);
    	}
    	#endregion
     
    	#region GetRankedConcepts
    	public string URLGetRankedConcepts(string url)
    	{
    		CheckURL(url);

    		return URLGetRankedConcepts(url, new AlchemyAPI_ConceptParams());
    	}

    	public string URLGetRankedConcepts(string url, AlchemyAPI_ConceptParams parameters)
    	{
    		CheckURL(url);
    		parameters.setUrl(url);

    		return GET("URLGetRankedConcepts", "url", parameters);
    	}

    	public string HTMLGetRankedConcepts(string html, string url)
    	{
    		CheckHTML(html, url);

    		return HTMLGetRankedConcepts(html, url, new AlchemyAPI_ConceptParams());
    	}

    	public string HTMLGetRankedConcepts(string html, string url, AlchemyAPI_ConceptParams parameters)
    	{
    		CheckHTML(html, url);
    		parameters.setHtml(html);
    		parameters.setUrl(url);

    		return POST("HTMLGetRankedConcepts", "html", parameters);
    	}

    	public string TextGetRankedConcepts(string text)
    	{
    		CheckText(text);

    		return TextGetRankedConcepts(text,new AlchemyAPI_ConceptParams());
    	}

    	public string TextGetRankedConcepts(string text, AlchemyAPI_ConceptParams parameters)
    	{
    		CheckText(text);
    		parameters.setText(text);

    		return POST("TextGetRankedConcepts", "text", parameters);
    	}
    	#endregion
    	
    	#region GetRankedKeywords
    	public string URLGetRankedKeywords(string url)
    	{
    		CheckURL(url);

    		return URLGetRankedKeywords(url, new AlchemyAPI_KeywordParams());
    	}

    	public string URLGetRankedKeywords(string url, AlchemyAPI_KeywordParams parameters)
    	{
    		CheckURL(url);
    		parameters.setUrl(url);

    		return GET("URLGetRankedKeywords", "url", parameters);
    	}

    	public string HTMLGetRankedKeywords(string html, string url)
    	{
    		CheckHTML(html, url);

    		return HTMLGetRankedKeywords(html, url, new AlchemyAPI_KeywordParams());
    	}

    	public string HTMLGetRankedKeywords(string html, string url, AlchemyAPI_KeywordParams parameters)
    	{
    		CheckHTML(html, url);
    		parameters.setHtml(html);
    		parameters.setUrl(url);

    		return POST("HTMLGetRankedKeywords", "html", parameters);
    	}

    	public string TextGetRankedKeywords(string text)
    	{
    		CheckText(text);

    		return TextGetRankedKeywords(text,new AlchemyAPI_KeywordParams());
    	}

    	public string TextGetRankedKeywords(string text, AlchemyAPI_KeywordParams parameters)
    	{
    		CheckText(text);
    		parameters.setText(text);

    		return POST("TextGetRankedKeywords", "text", parameters);
    	}
    	#endregion

    	#region GetLanguage
    	public string URLGetLanguage(string url)
    	{
    		CheckURL(url);

    		return URLGetLanguage(url,new AlchemyAPI_LanguageParams());
    	}

    	public string URLGetLanguage(string url, AlchemyAPI_LanguageParams parameters)
    	{
    		CheckURL(url);
    		parameters.setUrl(url);

    		return GET("URLGetLanguage", "url", parameters);
    	}

    	public string HTMLGetLanguage(string html, string url)
    	{
    		CheckHTML(html, url);

    		return HTMLGetLanguage(html,url,new AlchemyAPI_LanguageParams());
    	}

    	public string HTMLGetLanguage(string html, string url, AlchemyAPI_LanguageParams parameters)
    	{
    		CheckHTML(html, url);
    		parameters.setHtml(html);
    		parameters.setUrl(url);

    		return POST("HTMLGetLanguage", "html", parameters);
    	}

    	public string TextGetLanguage(string text)
    	{
    		CheckText(text);

    		return TextGetLanguage(text, new AlchemyAPI_LanguageParams());
    	}

    	public string TextGetLanguage(string text, AlchemyAPI_LanguageParams parameters)
    	{
    		CheckText(text);
    		parameters.setText(text);

    		return POST("TextGetLanguage", "text", parameters);
    	}
    	#endregion
    	
    	#region GetCategory
    	public string URLGetCategory(string url)
    	{
    		CheckURL(url);

    		return URLGetCategory(url, new AlchemyAPI_CategoryParams() );
    	}

    	public string URLGetCategory(string url, AlchemyAPI_CategoryParams parameters)
    	{
    		CheckURL(url);
    		parameters.setUrl(url);

    		return GET("URLGetCategory", "url", parameters);
    	}

    	public string HTMLGetCategory(string html, string url)
    	{
    		CheckHTML(html, url);

    		return HTMLGetCategory(html, url, new AlchemyAPI_CategoryParams());
    	}

    	public string HTMLGetCategory(string html, string url, AlchemyAPI_CategoryParams parameters)
    	{
    		CheckHTML(html, url);
    		parameters.setHtml(html);
    		parameters.setUrl(url);

    		return POST("HTMLGetCategory", "html", parameters);
    	}

    	public string TextGetCategory(string text)
    	{
    		CheckText(text);

    		return TextGetCategory(text, new AlchemyAPI_CategoryParams());
    	}

    	public string TextGetCategory(string text, AlchemyAPI_CategoryParams parameters)
    	{
    		CheckText(text);
    		parameters.setText(text);

    		return POST("TextGetCategory", "text", parameters);
    	}
    	#endregion
    	
    	#region GetText
    	public string URLGetText(string url)
    	{
    		CheckURL(url);

    		return URLGetText(url, new AlchemyAPI_TextParams());
    	}

    	public string URLGetText(string url, AlchemyAPI_TextParams parameters)
    	{
    		CheckURL(url);
    		parameters.setUrl(url);

    		return GET("URLGetText", "url", parameters);
    	}

    	public string HTMLGetText(string html, string url)
    	{
    		CheckHTML(html, url);

    		return HTMLGetText(html,url, new AlchemyAPI_TextParams());
    	}

    	public string HTMLGetText(string html, string url,AlchemyAPI_TextParams parameters)
    	{
    		CheckHTML(html, url);
    		parameters.setHtml(html);
    		parameters.setUrl(url);

    		return POST("HTMLGetText", "html", parameters);
    	}
    	#endregion

    	#region GetRawText
    	public string URLGetRawText(string url)
    	{
    		CheckURL(url);

    		return URLGetRawText(url, new AlchemyAPI_BaseParams());
    	}

    	public string URLGetRawText(string url, AlchemyAPI_BaseParams parameters)
    	{
    		CheckURL(url);
    		parameters.setUrl(url);

    		return GET("URLGetRawText", "url", parameters);
    	}

    	public string HTMLGetRawText(string html, string url)
    	{
    		CheckHTML(html, url);

    		return HTMLGetRawText(html, url, new AlchemyAPI_BaseParams());
    	}

    	public string HTMLGetRawText(string html, string url, AlchemyAPI_BaseParams parameters)
    	{
    		CheckHTML(html, url);
    		parameters.setHtml(html);
    		parameters.setUrl(url);

    		return POST("HTMLGetRawText", "html", parameters);
    	}
    	#endregion

    	#region GetTitle
    	public string URLGetTitle(string url)
    	{
    		CheckURL(url);

    		return URLGetTitle(url, new AlchemyAPI_BaseParams());
    	}

    	public string URLGetTitle(string url, AlchemyAPI_BaseParams parameters)
    	{
    		CheckURL(url);
    		parameters.setUrl(url);

    		return GET("URLGetTitle", "url", parameters);
    	}

    	public string HTMLGetTitle(string html, string url)
    	{
    		CheckHTML(html, url);

    		return HTMLGetTitle(html, url, new AlchemyAPI_BaseParams());
    	}

    	public string HTMLGetTitle(string html, string url, AlchemyAPI_BaseParams parameters)
    	
    	{
    		CheckHTML(html, url);
    		parameters.setHtml(html);
    		parameters.setUrl(url);

    		return POST("HTMLGetTitle", "html", parameters);
    	}
    	#endregion
    	
    	#region GetFeedLinks
    	public string URLGetFeedLinks(string url)
    	{
    		CheckURL(url);

    		return URLGetFeedLinks(url, new AlchemyAPI_BaseParams());
    	}

    	public string URLGetFeedLinks(string url, AlchemyAPI_BaseParams parameters)
    	{
    		CheckURL(url);
    		parameters.setUrl(url);

    		return GET("URLGetFeedLinks", "url", parameters);
    	}

    	public string HTMLGetFeedLinks(string html, string url)
    	{
    		CheckHTML(html, url);

    		return HTMLGetFeedLinks(html,url, new AlchemyAPI_BaseParams());
    	}

    	public string HTMLGetFeedLinks(string html, string url, AlchemyAPI_BaseParams parameters)
    	{
    		CheckHTML(html, url);
    		parameters.setHtml(html);
    		parameters.setUrl(url);

    		return POST("HTMLGetFeedLinks", "html", parameters);
    	}
    	#endregion
    	
    	#region GetMicroformats
    	public string URLGetMicroformats(string url)
    	{
    		CheckURL(url);

    		return URLGetMicroformats(url, new AlchemyAPI_BaseParams());
    	}

    	public string URLGetMicroformats(string url, AlchemyAPI_BaseParams parameters)
    	{
    		CheckURL(url);
    		parameters.setUrl(url);

    		return GET("URLGetMicroformatData", "url", parameters);
    	}

    	public string HTMLGetMicroformats(string html, string url)
    	{
    		CheckHTML(html, url);

    		return HTMLGetMicroformats(html,url, new AlchemyAPI_BaseParams());
    	}

    	public string HTMLGetMicroformats(string html, string url, AlchemyAPI_BaseParams parameters)
    	{
    		CheckHTML(html, url);
    		parameters.setHtml(html);
    		parameters.setUrl(url);

    		return POST("HTMLGetMicroformatData", "html", parameters);
    	}
    	#endregion

    	#region GetConstraintQuery
    	public string URLGetConstraintQuery(string url, string query)
    	{
    		CheckURL(url);
    		if (query.Length < 2)
    		{
    			System.ApplicationException ex =
    				new System.ApplicationException("Invalid constraint query specified.");

    			throw ex;
    		}

    		AlchemyAPI_ConstraintQueryParams cqParams = new AlchemyAPI_ConstraintQueryParams();
    		cqParams.setCQuery(query);

    		return URLGetConstraintQuery(url,cqParams);
    	}

    	public string URLGetConstraintQuery(string url, AlchemyAPI_ConstraintQueryParams parameters)
    	{
    		CheckURL(url);
    		if (parameters.getCQuery().Length < 2)
    		{
    			System.ApplicationException ex =
    				new System.ApplicationException("Invalid constraint query specified.");

    			throw ex;
    		}
    	
    		parameters.setUrl(url);

    		return POST("URLGetConstraintQuery", "url", parameters);
    	}

    	public string HTMLGetConstraintQuery(string html, string url, string query)
    	{
    		CheckHTML(html, url);
    		if (query.Length < 2)
    		{
    			System.ApplicationException ex =
    				new System.ApplicationException("Invalid constraint query specified.");

    			throw ex;
    		}

    		AlchemyAPI_ConstraintQueryParams cqParams = new AlchemyAPI_ConstraintQueryParams();
    		cqParams.setCQuery(query);

    		return HTMLGetConstraintQuery(html, url, cqParams);
    	}

    	public string HTMLGetConstraintQuery(string html, string url, AlchemyAPI_ConstraintQueryParams parameters)
    	{
    		CheckHTML(html, url);
    		if (parameters.getCQuery().Length < 2)
    		{
    			System.ApplicationException ex =
    				new System.ApplicationException("Invalid constraint query specified.");

    			throw ex;
    		}

    		parameters.setHtml(html);
    		parameters.setUrl(url);

    		return POST("HTMLGetConstraintQuery", "html", parameters);
    	}
    	#endregion
    	
    	#region GetTextSentiment
    	public string URLGetTextSentiment(string url)
    	{
    		CheckURL(url);

    		return URLGetTextSentiment(url, new AlchemyAPI_BaseParams());
    	}

    	public string URLGetTextSentiment(string url, AlchemyAPI_BaseParams parameters)
    	{
    		CheckURL(url);
    		parameters.setUrl(url);

    		return GET("URLGetTextSentiment", "url", parameters);
    	}

    	public string HTMLGetTextSentiment(string html, string url)
    	{
    		CheckHTML(html, url);

    		return HTMLGetTextSentiment(html, url, new AlchemyAPI_BaseParams());
    	}

    	public string HTMLGetTextSentiment(string html, string url, AlchemyAPI_BaseParams parameters)
    	{
    		CheckHTML(html, url);
    		parameters.setHtml(html);
    		parameters.setUrl(url);

    		return POST("HTMLGetTextSentiment", "html", parameters);
    	}

    	public string TextGetTextSentiment(string text)
    	{
    		CheckText(text);

    		return TextGetTextSentiment(text,new AlchemyAPI_BaseParams());
    	}

    	public string TextGetTextSentiment(string text, AlchemyAPI_BaseParams parameters)
    	{
    		CheckText(text);
    		parameters.setText(text);

    		return POST("TextGetTextSentiment", "text", parameters);
    	}
    	#endregion

    	#region GetTargetedSentiment
    	public string URLGetTargetedSentiment(string url, string target)
    	{
    		CheckURL(url);
    		CheckText(target);
    			
    		return URLGetTargetedSentiment(url, target, new AlchemyAPI_TargetedSentimentParams());
    	}
    		
    	public string URLGetTargetedSentiment(string url, string target, AlchemyAPI_TargetedSentimentParams parameters)
    	{
    		CheckURL(url);
    		CheckText(target);
    			
    		parameters.setUrl(url);
    		parameters.setTarget(target);
    			
    		return GET("URLGetTargetedSentiment", "url", parameters);
    	}
    		
    	public string HTMLGetTargetedSentiment(string html, string url, string target)
    	{
    		CheckHTML(html, url);
    		CheckText(target);
    			
    		return HTMLGetTargetedSentiment(html, url, target, new AlchemyAPI_TargetedSentimentParams());
    	}
    		
    	public string HTMLGetTargetedSentiment(string html, string url, string target, AlchemyAPI_TargetedSentimentParams parameters)
    	{
    		
    		CheckHTML(html, url);
    		CheckText(target);
    			
    		parameters.setHtml(html);
    		parameters.setUrl(url);
    		parameters.setTarget(target);
    			
    		return POST("HTMLGetTargetedSentiment", "html", parameters);
    	}
    	
    	public string TextGetTargetedSentiment(string text, string target)
    	{
    		CheckText(text);
    		CheckText(target);
    			
    		return TextGetTargetedSentiment(text, target, new AlchemyAPI_TargetedSentimentParams());
    	}
    		
    	public string TextGetTargetedSentiment(string text, string target, AlchemyAPI_TargetedSentimentParams parameters)
    	{
    		CheckText(text);
    		CheckText(target);
    			
    		parameters.setText(text);
    		parameters.setTarget(target);
    			
    		return POST("TextGetTargetedSentiment", "text", parameters);
    	}
    	#endregion

    	#region GetRelations
    	public string URLGetRelations(string url)
    	{
    		CheckURL(url);

    		return URLGetRelations(url, new AlchemyAPI_RelationParams());
    	}

    	public string URLGetRelations(string url, AlchemyAPI_RelationParams parameters)
    	{
    		CheckURL(url);
    		parameters.setUrl(url);

    		return GET("URLGetRelations", "url", parameters);
    	}

    	public string HTMLGetRelations(string html, string url)
    	{
    		CheckHTML(html, url);

    		return HTMLGetRelations(html, url, new AlchemyAPI_RelationParams());
    	}

    	public string HTMLGetRelations(string html, string url, AlchemyAPI_RelationParams parameters)
    	{
    		CheckHTML(html, url);
    		parameters.setHtml(html);
    		parameters.setUrl(url);

    		return POST("HTMLGetRelations", "html", parameters);
    	}

    	public string TextGetRelations(string text)
    	{
    		CheckText(text);

    		return TextGetRelations(text,new AlchemyAPI_RelationParams());
    	}

    	public string TextGetRelations(string text, AlchemyAPI_RelationParams parameters)
    	{
    		CheckText(text);
    		parameters.setText(text);

    		return POST("TextGetRelations", "text", parameters);
    	}
    	#endregion

    	#region GetCombinedData
    	public string URLGetCombinedData(string url, AlchemyAPI_CombinedDataParams parameters)
    	{
    		CheckURL(url);
    		parameters.setUrl(url);

    		return GET ("URLGetCombinedData", "url", parameters);
    	}
    	public string TextGetCombinedData(string text, AlchemyAPI_CombinedDataParams parameters)
    	{
    		CheckText(text);
    		parameters.setText(text);

    		return POST ("TextGetCombinedData", "text", parameters);
    	}
    	#endregion
    	
    	#region GetRankedTaxonomy
    	public string URLGetRankedTaxonomy (string url)
    	{
    		return URLGetRankedTaxonomy(url, new AlchemyAPI_TaxonomyParams());
    	}
    	public string URLGetRankedTaxonomy (string url, AlchemyAPI_TaxonomyParams parameters)
    	{
    		CheckURL(url);
    		parameters.setUrl(url);

    		return GET ("URLGetRankedTaxonomy", "url", parameters);
    	}
    	public string HTMLGetRankedTaxonomy (string html, string url)
    	{
    		return HTMLGetRankedTaxonomy(html, url, new AlchemyAPI_TaxonomyParams());
    	}
    	public string HTMLGetRankedTaxonomy (string html, string url, AlchemyAPI_TaxonomyParams parameters)
    	{
    		CheckHTML(html, url);
    		parameters.setHtml(html);
    		parameters.setUrl(url);

    		return POST ("HTMLGetRankedTaxonomy", "html", parameters);
    	}
    	public string TextGetRankedTaxonomy (string text)
    	{
    		return TextGetRankedTaxonomy(text, new AlchemyAPI_TaxonomyParams());
    	}
    	public string TextGetRankedTaxonomy (string text, AlchemyAPI_TaxonomyParams parameters)
    	{
    		CheckText(text);
    		parameters.setText(text);

    		return POST ("TextGetRankedTaxonomy", "text", parameters);
    	}
    	#endregion

    	#region GetImage
    	public string HTMLGetImage (string html, string url)
    	{
    		return HTMLGetImage(html, url, new AlchemyAPI_ImageParams());
    	}
    	public string HTMLGetImage (string html, string url, AlchemyAPI_ImageParams parameters)
    	{
    		CheckHTML(html, url);
    		parameters.setHtml(html);
    		parameters.setUrl(url);

    		return POST ("HTMLGetImage", "html", parameters);
    	}
    	public string URLGetImage (string url)
    	{
    		return URLGetImage (url, new AlchemyAPI_ImageParams());
    	}
    	public string URLGetImage (string url, AlchemyAPI_ImageParams parameters)
    	{
    		CheckURL(url);
    		parameters.setUrl(url);

    		return GET ("URLGetImage", "url", parameters);
    	}
    	#endregion
       
        #region GetRankedImageKeywords
        public string URLGetRankedImageKeywords(string url)
        {
            AlchemyAPI_RankedImageKeywords pms = new AlchemyAPI_RankedImageKeywords
            {
                ImageURL = url
            };

            return URLGetRankedImageKeywords(pms);
        }
        public string URLGetRankedImageKeywords(AlchemyAPI_RankedImageKeywords parameters)
        {
            CheckURL(parameters.ImageURL);

            return GET("URLGetRankedImageKeywords", "url", parameters);
        }
        public string ImageGetRankedImageKeywords(AlchemyAPI_RankedImageKeywords parameters)
        {
            return GET("ImageGetRankedImageKeywords", "image", parameters);
        }
        #endregion

	    private void CheckHTML(string html, string url)
        {
            if (html.Length < 10)
            {
                System.ApplicationException ex =
				new System.ApplicationException ("Enter a HTML document to analyze.");

                throw ex;
            }

            if (url.Length < 10)
            {
                System.ApplicationException ex =
				new System.ApplicationException ("Enter a web URL to analyze.");

                throw ex;
            }
        }

	    private void CheckText(string text)
        {
            if (text.Length < 5)
            {
                System.ApplicationException ex =
				new System.ApplicationException ("Enter some text to analyze.");

                throw ex;
            }
        }

	    private void CheckURL(string url)
        {
            if (url.Length < 10)
            {
                System.ApplicationException ex =
				new System.ApplicationException ("Enter a web URL to analyze.");

                throw ex;
            }
        }

	    private string GET(string callName, string callPrefix, AlchemyAPI_BaseParams parameters)
        { // callMethod, callPrefix, ... params
            StringBuilder uri = new StringBuilder ();
            uri.Append(_requestUri).Append(callPrefix).Append("/").Append(callName);
            uri.Append("?apikey=").Append(_apiKey).Append(parameters.getParameterString());

            parameters.resetBaseParams();

            Uri address = new Uri (uri.ToString());
            HttpWebRequest wreq = WebRequest.Create(address) as HttpWebRequest;
            wreq.Proxy = GlobalProxySelection.GetEmptyWebProxy();

            byte[] postData = parameters.GetPostData();

            if (postData == null)
            {
                wreq.Method = "GET";
            }
            else
            {
                wreq.Method = "POST";
                using (var ps = wreq.GetRequestStream())
                {
                    ps.Write(postData, 0, postData.Length);
                }
            }

            return DoRequest(wreq, parameters.getOutputMode());
        }

	    private string POST(string callName, string callPrefix, AlchemyAPI_BaseParams parameters)
        { // callMethod, callPrefix, ... params
            Uri address = new Uri (_requestUri + callPrefix + "/" + callName);

            HttpWebRequest wreq = WebRequest.Create(address) as HttpWebRequest;
            wreq.Proxy = GlobalProxySelection.GetEmptyWebProxy();
            wreq.Method = "POST";
            wreq.ContentType = "application/x-www-form-urlencoded";

            StringBuilder d = new StringBuilder ();
            d.Append("apikey=").Append(_apiKey).Append(parameters.getParameterString());

            parameters.resetBaseParams();

            byte[] bd = UTF8Encoding.UTF8.GetBytes(d.ToString());

            wreq.ContentLength = bd.Length;
            using (Stream ps = wreq.GetRequestStream())
            {
                ps.Write(bd, 0, bd.Length);
            }

            return DoRequest(wreq, parameters.getOutputMode());
        }

	    private string DoRequest(HttpWebRequest wreq, AlchemyAPI_BaseParams.OutputMode outputMode)
        {
            using (HttpWebResponse wres = wreq.GetResponse() as HttpWebResponse)
            {
                StreamReader r = new StreamReader (wres.GetResponseStream());

                string xml = r.ReadToEnd();

                if (string.IsNullOrEmpty(xml))
                    throw new XmlException ("The API request returned back an empty response. Please verify that the url is correct.");
			
                XmlDocument xmlDoc = new XmlDocument ();
                xmlDoc.LoadXml(xml);

                XmlElement root = xmlDoc.DocumentElement;

                if (AlchemyAPI_BaseParams.OutputMode.XML == outputMode)
                {
                    XmlNode status = root.SelectSingleNode("/results/status");

                    if (status.InnerText != "OK")
                    {
                        System.ApplicationException ex =
						new System.ApplicationException ("Error making API call.");

                        throw ex;
                    }
                }
                else if (AlchemyAPI_BaseParams.OutputMode.RDF == outputMode)
                {
                    XmlNamespaceManager nm = new XmlNamespaceManager (xmlDoc.NameTable);
                    nm.AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
                    nm.AddNamespace("aapi", "http://rdf.alchemyapi.com/rdf/v1/s/aapi-schema#");
                    XmlNode status = root.SelectSingleNode("/rdf:RDF/rdf:Description/aapi:ResultStatus", nm);

                    if (status.InnerText != "OK")
                    {
                        System.ApplicationException ex =
						new System.ApplicationException ("Error making API call.");

                        throw ex;
                    }
                }

                return xml;
		
            }
        }
    }

}

