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
	public enum PageImageMode
	{
		TrustMetadata,
		AlwaysInfer,
		OnlyMetadata
	};

	public static class PageImageModeHelper
	{
		public static string ToString (PageImageMode mode)
		{
			switch (mode) 
			{
			case PageImageMode.TrustMetadata:
				return "trust-metadata";
			case PageImageMode.AlwaysInfer:
				return "always-infer";
			case PageImageMode.OnlyMetadata:
				return "only-metadata";
			default:
				throw new ArgumentException("The specified image mode '" + mode + "' is not supported.");
			}
		}
	}

	public class AlchemyAPI_ImageParams : AlchemyAPI_BaseParams
	{
		private PageImageMode? _imageMode;
		private string _jsonpCallback;

		public PageImageMode? ImageMode {
		get { return _imageMode; }
		set { _imageMode = value; }
		}

		public string JsonPCallback {
		get { return _jsonpCallback; }
		set { _jsonpCallback = value; }
		}

		public override string getParameterString ()
		{
			string retString = base.getParameterString ();

			if (_imageMode != null)
				retString += "&extractMode=" + PageImageModeHelper.ToString(_imageMode.Value);

			if (_jsonpCallback != null)
				retString += "&jsonp=" + _jsonpCallback;

			return retString;
		}
	}
}
