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

namespace LoggerReceptor
{
	/// <summary>
	/// This receptor is an edge receptor, receiving DebugMessage carriers and outputting them to a logging window.
	/// </summary>
	public class ReceptorDefinition : BaseReceptor
	{
		public override string Name { get { return "Logger"; } }
		public override bool IsEdgeReceptor { get { return true; } }

		public ReceptorDefinition(IReceptorSystem rsys) : base(rsys)
		{
			AddReceiveProtocol("LoggerMessage", (Action<dynamic>)(signal => Flyout(signal.TextMessage.Text.Value)));
		}

		/// <summary>
		/// A visualization at the system level.
		/// </summary>
		/// <param name="msg"></param>
		protected void Flyout(string msg)
		{
			CreateCarrier("SystemMessage", signal =>
				{
					signal.Action = "Flyout";
					signal.Data = msg;
					signal.Source = this;
				});
		}
	}
}
