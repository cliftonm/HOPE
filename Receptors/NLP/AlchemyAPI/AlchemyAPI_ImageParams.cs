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