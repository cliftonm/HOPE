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
