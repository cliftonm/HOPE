using System;
using System.Web;

namespace AlchemyAPI
{
	public class AlchemyAPI_BaseParams
	{
		public enum OutputMode
		{
			NONE,
			XML,
			JSON,
			RDF,
			RelTag,
			RelTagRaw
		};

		private String url;
		private String html;
		private String text;
		private OutputMode outputMode = OutputMode.XML;
		private String customParameters;
	
		public String getUrl()
		{
			return url;
		}

		public AlchemyAPI_BaseParams setUrl(String url)
		{
			this.url = url;
			return this;
		}

		public String getHtml()
		{
			return html;
		}

		public AlchemyAPI_BaseParams setHtml(String html)
		{
			this.html = html;
			return this;
		}

		public String getText()
		{
			return text;
	   	}

		public AlchemyAPI_BaseParams setText(String text)
		{
			this.text = text;
			return this;
		}

		public OutputMode getOutputMode()
		{
			return outputMode;
		}

		public AlchemyAPI_BaseParams setOutputMode(OutputMode outputMode)
		{
			this.outputMode = outputMode;
			return this;
		}

		public String getCustomParameters()
		{
			return customParameters;
		}

		public AlchemyAPI_BaseParams setCustomParameters(params object[] argsRest)
		{
			string returnString = "";

			for (int i = 0; i < argsRest.Length; ++i)
			{
				returnString = returnString + '&' + argsRest[i];
				if (++i < argsRest.Length)
					returnString = returnString + '=' + HttpUtility.UrlEncode((string)argsRest[i]);
			}

			this.customParameters = returnString;
			return this;
		}

		public void resetBaseParams()
		{
			url = null;
			html = null;
			text = null;
		}

		public virtual String getParameterString()
		{
			String retString = "";

			if (url != null) retString += "&url=" + HttpUtility.UrlEncode(url);
			if (html != null) retString += "&html=" + HttpUtility.UrlEncode(html);
			if (text != null) retString += "&text=" + HttpUtility.UrlEncode(text);
			if (customParameters!=null) retString+=customParameters;
			if (outputMode != OutputMode.NONE)
			{
				retString += "&outputMode=";
				switch (outputMode)
				{
				case OutputMode.XML:
					retString += "xml";
					break;
				case OutputMode.JSON:
					retString += "json";
					break;
				case OutputMode.RDF:
					retString += "rdf";
					break;
				case OutputMode.RelTag:
					retString += "rel-tag";
					break;
				case OutputMode.RelTagRaw:
					retString += "rel-tag-raw";
					break;
				default:
					throw new ArgumentException("Unsupported output mode '" + outputMode + "'.");
				}
			} 

			return retString;
		}

        public virtual byte[] GetPostData()
        {
            return null;
        }

		protected string encodeBool(bool? val)
		{
			if (val.HasValue && val.Value)
				return "1";
			else
				return "0";
		}
	}
}
