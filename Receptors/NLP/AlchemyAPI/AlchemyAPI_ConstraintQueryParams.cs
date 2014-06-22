using System;
using System.Web;


namespace AlchemyAPI
{
	public class AlchemyAPI_ConstraintQueryParams : AlchemyAPI_BaseParams
	{
		private string cQuery;

		public string getCQuery()
		{
			return cQuery;
		}
		public void setCQuery(string cQuery)
		{
			this.cQuery = cQuery;
		}
		override public string getParameterString()
		{
			string retstring = base.getParameterString();
			if (cQuery != null) retstring += "&cquery=" + HttpUtility.UrlEncode(cQuery);
			
			return retstring;
		}
	}
}
