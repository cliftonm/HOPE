using System;
using System.Web;

namespace AlchemyAPI
{
	public class AlchemyAPI_LanguageParams : AlchemyAPI_BaseParams
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

		private SourceTextMode sourceText;
		private string cQuery;
		private string xPath;
		
		public SourceTextMode getSourceText()
		{
			return sourceText;
		}
		
		public void setSourceText(SourceTextMode sourceText)
		{
			this.sourceText = sourceText;
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
			if (cQuery != null) retString += "&cquery=" + HttpUtility.UrlEncode(cQuery);
			if (xPath != null) retString += "&xpath=" + HttpUtility.UrlEncode(xPath);

			return retString;
		}
	}
}
