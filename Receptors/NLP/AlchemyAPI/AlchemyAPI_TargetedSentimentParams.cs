using System;
using System.Web;

namespace AlchemyAPI
{	
	public class AlchemyAPI_TargetedSentimentParams : AlchemyAPI_BaseParams
	{
		
		private enum TBOOL
		{
			NONE,
			FALSE,
			TRUE
		}
		
		private TBOOL showSourceText;
		private string target;
	
		public bool isshowSourceText()
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
		
		public string getTarget()
		{
			return target;
		}
		
		public void setTarget(string target)
		{
			this.target = target;
		}
		
		override public String getParameterString()
		{
			String retString = base.getParameterString();
   
			if (showSourceText != TBOOL.NONE) retString += "&showSourceText=" + (showSourceText ==TBOOL.TRUE ? "1" : "0");
			if (target != null) retString += "&target=" + HttpUtility.UrlEncode(target);

			return retString;
		}
	}
}

