using System;
using System.Web;

namespace AlchemyAPI
{
	public class AlchemyAPI_TextParams : AlchemyAPI_BaseParams
	{
		private enum TBOOL
		{
			NONE,
			FALSE,
			TRUE
		}

		private TBOOL useMetadata;
		private TBOOL extractLinks;
	
		public bool isUseMetadata()
		{
			if (TBOOL.TRUE == useMetadata)
				return true;
			return false;
		}

		public void setUseMetadata(bool useMetadata)
		{
			if (useMetadata)
				this.useMetadata = TBOOL.TRUE;
			else
				this.useMetadata = TBOOL.FALSE;
		}

		public bool isExtractLinks()
		{
			if (TBOOL.TRUE == extractLinks)
				return true;
			return false;
		}

		public void setExtractLinks(bool extractLinks)
		{
			if (extractLinks)
				this.extractLinks = TBOOL.TRUE;
			else
				this.extractLinks = TBOOL.FALSE;
		}

		override public String getParameterString()
		{
			String retString = base.getParameterString();
   
			if (useMetadata != TBOOL.NONE) retString += "&useMetadata=" + (useMetadata==TBOOL.TRUE ? "1" : "0");
			if (extractLinks != TBOOL.NONE) retString += "&extractLinks=" + (extractLinks == TBOOL.TRUE ? "1" : "0");

			return retString;
		}
	}
}
