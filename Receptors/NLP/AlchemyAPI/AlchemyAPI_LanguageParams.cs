/*
    Copyright 2104 Higher Order Programming

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

*/

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
