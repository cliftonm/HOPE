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

using Clifton.SemanticTypeSystem.Interfaces;

namespace Clifton.SemanticTypeSystem
{
	/// <summary>
	/// A container for an instance of the semantic type, including:
	/// The instance name
	/// The instance itself
	/// Any parent
	/// The Guid key used to locate the entry in the dictionary of instances.
	/// </summary>
	public class SemanticTypeInstance
	{
		public string Name { get; set; }
		public IRuntimeSemanticType Instance { get; set; }
		public IRuntimeSemanticType Parent { get; set; }
		public Guid Key { get; set; }
		public ISemanticType Definition { get; set; }
	}
}
