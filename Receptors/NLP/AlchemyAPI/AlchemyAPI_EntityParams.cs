using System;
using System.Web;

namespace AlchemyAPI
{
	public class AlchemyAPI_EntityParams : AlchemyAPI_BaseParams
	{
		public enum SourceTextMode
		{
			NONE,
			CLEANED_OR_RAW,
			CLEANED,
			RAW,
			CQUERY,
			XPATH
		}

		private enum TBOOL
		{
			NONE,
			FALSE,
			TRUE
		}

		private TBOOL disambiguate;
		private TBOOL linkedData;
		private TBOOL coreference;
		private TBOOL quotations;
		private int maxRetrieve = -1;
		private SourceTextMode sourceText;
		private TBOOL showSourceText;
		private string cQuery;
		private string xPath;
		private string baseUrl;
		private TBOOL sentiment;

		public bool isDisambiguate()
		{
			if (TBOOL.TRUE == disambiguate)
				return true;
			return false;
		}

		public void setDisambiguate(bool disambiguate)
		{
			if (disambiguate)
				this.disambiguate = TBOOL.TRUE;
			else
				this.disambiguate = TBOOL.FALSE;
		}

		public bool isLinkedData()
		{
			if (TBOOL.TRUE == linkedData)
				return true;
			return false;
		}

		public void setLinkedData(bool linkedData)
		{
			if (linkedData)
				this.linkedData = TBOOL.TRUE;
			else
				this.linkedData = TBOOL.FALSE;
		}

		public bool isCoreference()
		{
			if (TBOOL.TRUE == coreference)
				return true;
			return false;
		}

		public void setCoreference(bool coreference)
		{
			if (coreference)
				this.coreference = TBOOL.TRUE;
			else
				this.coreference = TBOOL.FALSE;
		}

		public bool isQuotations()
		{
			if (TBOOL.TRUE == quotations)
				return true;
			return false;
		}

		public void setQuotations(bool quotations)
		{
			if (quotations)
				this.quotations = TBOOL.TRUE;
			else
				this.quotations = TBOOL.FALSE;
		}
		
		
		public SourceTextMode getSourceText()
		{
			return sourceText;
		}
		
		public void setSourceText(SourceTextMode sourceText)
		{
			this.sourceText = sourceText;
		}
		
		public bool isShowSourceText()
		{
			if (TBOOL.TRUE == showSourceText)
				return true;
			return false;
		}
		
		public void setShowSourceText(bool showSourceText)
		{
			if (showSourceText)
				this.showSourceText = TBOOL.TRUE;
			else
				this.showSourceText = TBOOL.FALSE;
		}
		
		public string getCQuery()
		{
			return cQuery;
		}
		
		public void setCQuery(string cQuery)
		{
			this.cQuery = cQuery;
		}
		
		public string getXPath()
		{
			return xPath;
		}
		
		public void setXPath(string xPath)
		{
			this.xPath = xPath;
		}
		
		public int getMaxRetrieve()
		{
			return maxRetrieve;
		}
		
		public void setMaxRetrieve(int maxRetrieve)
		{
			this.maxRetrieve = maxRetrieve;
		}
		
		public bool isSentiment()
		{
			if (TBOOL.TRUE == sentiment)
				return true;
			return false;
		}
		
		public void setSentiment(bool sentiment)
		{
			if (sentiment)
				this.sentiment = TBOOL.TRUE;
			else
				this.sentiment = TBOOL.FALSE;
		}
		
		public string getBaseUrl()
		{
			return baseUrl;
		}
		
		public void setBaseUrl(string baseUrl)
		{
			this.baseUrl = baseUrl;
		}
		
		override public String getParameterString()
		{
			String retString = base.getParameterString();

			if (sourceText != SourceTextMode.NONE)
			{
				if (sourceText == SourceTextMode.CLEANED_OR_RAW)
					retString += "&sourceText=cleaned_or_raw";
				else if (sourceText == SourceTextMode.CLEANED)
					retString += "&sourceText=cleaned";
				else if (sourceText == SourceTextMode.RAW)
					retString += "&sourceText=raw";
				else if (sourceText == SourceTextMode.CQUERY)
					retString += "&sourceText=cquery";
				else if (sourceText == SourceTextMode.CQUERY)
					retString += "&sourceText=xpath";
			}
			if (showSourceText != TBOOL.NONE) retString += "&showSourceText=" + (showSourceText==TBOOL.TRUE ? "1" : "0");
			if (disambiguate != TBOOL.NONE) retString += "&disambiguate=" + (disambiguate == TBOOL.TRUE ? "1" : "0");
			if (linkedData != TBOOL.NONE) retString += "&linkedData=" + (linkedData == TBOOL.TRUE ? "1" : "0");
			if (coreference != TBOOL.NONE) retString += "&coreference=" + (coreference == TBOOL.TRUE ? "1" : "0");
			if (quotations != TBOOL.NONE) retString += "&quotations=" + (quotations == TBOOL.TRUE ? "1" : "0");
			if (sentiment != TBOOL.NONE) retString += "&sentiment=" + (sentiment == TBOOL.TRUE ? "1" : "0");
			if (cQuery != null) retString += "&cquery=" + HttpUtility.UrlEncode(cQuery);
			if (xPath != null) retString += "&xpath=" + HttpUtility.UrlEncode(xPath);
			if (maxRetrieve>-1) retString+="&maxRetrieve="+maxRetrieve;
			if (baseUrl != null) retString += "&baseUrl=" + HttpUtility.UrlEncode(baseUrl);
		   
			return retString;
		}
	}
}
