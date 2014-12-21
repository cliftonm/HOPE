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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

namespace APODEventGeneratorReceptor
{
	public class ReceptorDefinition : BaseReceptor
	{
		public override string Name { get { return "APOD Date Generator"; } }
		
		public ReceptorDefinition(IReceptorSystem rsys) : base(rsys)
		{
			AddEmitProtocol("ScrapeAPOD");
		}

		public override void Initialize()
		{
			DateTime start = DateTime.Parse("6/16/1995");
			// DateTime stop = start.AddMonths(1);
			DateTime stop = DateTime.Now;  

			for (DateTime date = start; date <= stop; date = date.AddDays(1))
			{
				ISemanticTypeStruct protocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("TimerEvent");
				dynamic signal = rsys.SemanticTypeSystem.Create("TimerEvent");
				signal.EventName = "ScrapeAPOD";
				signal.EventDateTime = date;
				rsys.CreateCarrier(this, protocol, signal);
			}

			rsys.Remove(this);
		}
	}
}
