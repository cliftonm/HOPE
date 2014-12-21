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
	public enum TaxSourceText
	{
		CleanedOrRaw,
		Cleaned,
		Raw,
		CQuery,
		XPath
	}

	public class AlchemyAPI_TaxonomyParams : AlchemyAPI_BaseParams
	{
		private string _jsonp;
		private bool? _showSourceText;
		private TaxSourceText? _sourceText;
		private string _cquery;
		private string _xpath;
		private string _baseUrl;

		public string JsonP {
		get { return _jsonp; }
		set { _jsonp = value; }
		}

		public bool? ShowSourceText {
		get { return _showSourceText; }
		set { _showSourceText = value; }
		}

		public TaxSourceText? SourceText {
		get { return _sourceText; }
		set { _sourceText = value; }
		}

		public string CQuery {
		get { return _cquery; }
		set { _cquery = value; }
		}

		public string XPath {
		get { return _xpath; }
		set { _xpath = value; }
		}

		public string BaseUrl {
		get { return _baseUrl; }
		set { _baseUrl = value; }
		}

		public override string getParameterString ()
		{
			string retString = base.getParameterString ();

			if (_jsonp != null)
				retString += "&jsonp=" + _jsonp;
			if (_showSourceText != null)
				retString += "&showSourceText=" + encodeBool(_showSourceText);
			if (_sourceText != null)
				retString += "&sourceText=" + encodeSourceText();
			if (_cquery != null)
				retString += "&cquery=" + _cquery;
			if (_xpath != null)
				retString += "&xpath=" + _xpath;
			if (_baseUrl != null)
				retString += "&baseUrl=" + _baseUrl;

			return retString;
		}

		private string encodeSourceText ()
		{
			switch (_sourceText.Value) 
			{
			case TaxSourceText.CleanedOrRaw:
				return "cleaned_or_raw";
			case TaxSourceText.Cleaned:
				return "cleaned";
			case TaxSourceText.Raw:
				return "raw";
			case TaxSourceText.CQuery:
				return "cquery";
			case TaxSourceText.XPath:
				return "xpath";
			default:
				throw new NotSupportedException("The specified source text '" + _sourceText.Value + "' is not supported.");
			}
		}
	}
}
