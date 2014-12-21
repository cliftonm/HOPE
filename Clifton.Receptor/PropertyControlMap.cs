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

namespace Clifton.Receptor
{
	/// <summary>
	/// Support for declaratively describing mappings between user configurable properties ([UserConfigurableProperty("Feed URL:")]) and controls
	/// in declarative forms instantiated by MycroParser.  While this structure is rather simple at the moment, it may have additional properties
	/// and features at some point to further refine data binding, limits, ranges, text input options, etc.
	/// </summary>
	public class PropertyControlMap
	{
		protected List<PropertyControlEntry> entries;

		public List<PropertyControlEntry> Entries
		{
			get { return entries; }
			set { entries = value; }
		}

		public PropertyControlMap()
		{
			entries = new List<PropertyControlEntry>();
		}
	}

	public class PropertyControlEntry
	{
		public string PropertyName { get; set; }
		public string ControlName { get; set; }
		public string ControlPropertyName { get; set; }

		public PropertyControlEntry()
		{
		}
	}
}
