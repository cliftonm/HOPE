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


	///
	/// Enumeration that specifies what to extract from the text.
	/// Multiple extractions are specified via a bitmask.
	/// 
	/// e.g.
	/// If you want to get title, author, and taxonomy, it would look like:
	/// Extract::Title | Extract::Author | Extract::Taxonomy
	///
	public enum CombinedExtract
	{
		PageImage = 0x1,
		Entity = 0x2,
		Keyword = 0x4,
		Title = 0x8,
		Author = 0x10,
		Taxonomy = 0x20,
		Concept = 0x40,
		Relation = 0x80,
		DocSentiment = 0x100
	};

	public class AlchemyAPI_CombinedDataParams : AlchemyAPI_BaseParams
	{
		private PageImageMode? _imageMode;
		private CombinedExtract _extract;
		private string _jsonpCallback;
		private bool? _disambiguateEntities;
		private bool? _includeLinkedData;
		private bool? _coreference;
		private bool? _quotations;
		private bool? _sentiment;
		private bool? _showSourceText;
		private int? _maxRetrieve;
		private string _baseUrl;

		public CombinedExtract Extractions {
		get {
			return _extract;
		}
		set {
			_extract = value;
		}
		}
		public PageImageMode? ImageMode {
		get { return _imageMode; }
		set { _imageMode = value; }
		}

		public string JsonPCallback {
		get { return _jsonpCallback; }
		set { _jsonpCallback = value; }
		}

		public bool? DisambiguateEntities {
		get { return _disambiguateEntities; }
		set { _disambiguateEntities = value; }
		}

		public bool? IncludeLinkedData {
		get { return _includeLinkedData; }
		set { _includeLinkedData = value; }
		}

		public bool? Coreference {
		get { return _coreference; }
		set { _coreference = value; }
		}

		public bool? Quotations {
		get { return _quotations; }
		set { _quotations = value; }
		}

		public bool? Sentiment {
		get { return _sentiment; }
		set { _sentiment = value; }
		}

		public bool? ShowSourceText {
		get { return _showSourceText; }
		set { _showSourceText = value; }
		}

		public int? MaxRetrieve {
		get { return _maxRetrieve; }
		set { _maxRetrieve = value; }
		}

		public string BaseURL {
		get { return _baseUrl; }
		set { _baseUrl = value; }
		}

		public bool isExtracting (CombinedExtract type)
		{
			return (_extract & type) == type;
		}
		public AlchemyAPI_CombinedDataParams setExtraction(CombinedExtract type, bool doExtraction)
		{
			if (doExtraction)
				_extract |= type;
			else
				_extract &= ~type;
			return this;
		}

		public override string getParameterString ()
		{
			string retString = base.getParameterString ();

			retString += "&extract=" + getExtractionString();

			if (_imageMode != null)
				retString += "&extractMode=" + PageImageModeHelper.ToString(_imageMode.Value);

			if (_jsonpCallback != null)
				retString += "&jsonp=" + _jsonpCallback;

			if (_disambiguateEntities != null)
				retString += "&disambiguate=" + encodeBool(_disambiguateEntities);

			if (_includeLinkedData != null)
				retString += "&linkedData=" + encodeBool(_includeLinkedData);

			if (_coreference != null)
				retString += "&coreference=" + encodeBool(_coreference);

			if (_quotations != null)
				retString += "&quotations=" + encodeBool(_quotations);

			if (_sentiment != null)
				retString += "&sentiment=" + encodeBool(_sentiment);

			if (_showSourceText != null)
				retString += "&showSourceText=" + encodeBool(_showSourceText);

			if (_maxRetrieve != null)
				retString += "&maxRetrieve=" + _maxRetrieve;

			if (_baseUrl != null)
				retString += "&baseUrl=" + _baseUrl;

			return retString;
		}

		private string getExtractionString()
		{
			string ret = string.Empty;
			if (isExtracting(CombinedExtract.PageImage))
				ret += "page-image,";
			if (isExtracting(CombinedExtract.Entity))
				ret += "entity,";
			if (isExtracting(CombinedExtract.Keyword))
				ret += "keyword,";
			if (isExtracting(CombinedExtract.Title))
				ret += "title,";
			if (isExtracting(CombinedExtract.Author))
				ret += "author,";
			if (isExtracting(CombinedExtract.Taxonomy))
				ret += "taxonomy,";
			if (isExtracting(CombinedExtract.Concept))
				ret += "concept,";
			if (isExtracting(CombinedExtract.Relation))
				ret += "relation,";
			if (isExtracting(CombinedExtract.DocSentiment))
				ret += "doc-sentiment";

			// Trim the trailing comma
			if (ret.EndsWith(","))
				ret = ret.Remove(ret.Length - 1);

			if (ret.Length == 0)
				throw new ArgumentException("At least one extraction must be specified.");

			return ret;
		}
	}
}
