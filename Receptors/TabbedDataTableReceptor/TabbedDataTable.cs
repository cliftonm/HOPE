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

using Clifton.ExtensionMethods;
using Clifton.Tools.Strings.Extensions;

using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

namespace TabbedDataTableReceptor
{
	public class TabbedDataTable : BaseReceptor
    {
		// Always override the Name property.
		public override string Name { get { return "Tabbed DataTable"; } }

		// Other optional property overides...

		/// <summary>
		/// Constructor called when the class is instantiated.
		/// Do not make any assumptions about the existence of other receptors at this point.
		/// Typically, all we do here is register emitted and received protocols.
		/// </summary>
		public TabbedDataTable(IReceptorSystem rsys)
			: base(rsys)
		{
			// Protocols emitted by this receptor
			// AddEmitProtocol("[protocol name]");

			// Protocols received by this receptor
			 AddReceiveProtocol("[protocol name]",
				 // Qualifier:
				 signal =>
				 {
					 // Put any qualifiers here to filter specific signals based on signal data.
					 // Replace the "return true" with a bool result.
					 return true;
				 },
				 // Action:
				signal =>
				{
					// Put any actions, method calls, etc., when the qualified signal is received.
				});
		}

		/// <summary>
		/// Any custom initialization after instantiation goes here.
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();
		}

		/// <summary>
		/// Any custom initialization after all receptors in the applet have been loaded goes here.
		/// </summary>
		public override void EndSystemInit()
		{
			base.EndSystemInit();
		}
    }
}
