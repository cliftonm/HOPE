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

