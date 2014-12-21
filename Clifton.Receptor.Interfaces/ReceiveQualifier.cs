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

namespace Clifton.Receptor.Interfaces
{
	/// <summary>
	/// Associates a qualifier with a protocol of interest.
	/// </summary>
	public class ReceiveQualifier
	{
		public string Protocol { get; set; }
		public Func<dynamic, bool> Qualifier { get; set; }
		public Action<dynamic> Action { get; set; }
		public bool Enabled { get; set; }

		/// <summary>
		/// Default constructor creates an always true qualifier.
		/// </summary>
		public ReceiveQualifier()
		{
			Qualifier = (_) => true;
			Action = (_) => { };
			Enabled = true;
		}

		/// <summary>
		/// Protocol name only, creates an always true qualifier.
		/// </summary>
		public ReceiveQualifier(string protocol)
		{
			Protocol = protocol;
			Qualifier = (_) => true;
			Action = (_) => { };
			Enabled = true;
		}

		/// <summary>
		/// Protocol name only, creates an always true qualifier with the specified action.
		/// </summary>
		public ReceiveQualifier(string protocol, Action<dynamic> action)
		{
			Protocol = protocol;
			Qualifier = (_) => true;
			Action = action;
			Enabled = true;
		}

		/// <summary>
		/// Specify the protocol name and qualifier.
		/// </summary>
		public ReceiveQualifier(string protocol, Func<dynamic, bool> qualifier)
		{
			Protocol = protocol;
			Qualifier = qualifier;
			Action = (_) => { };
			Enabled = true;
		}

		/// <summary>
		/// Specify the protocol name and qualifier and with the specified action.
		/// </summary>
		public ReceiveQualifier(string protocol, Func<dynamic, bool> qualifier, Action<dynamic> action)
		{
			Protocol = protocol;
			Qualifier = qualifier;
			Action = action;
			Enabled = true;
		}
	}
}
