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
	/// A semantic type declaration.
	/// </summary>
	public class SemanticTypeDecl : ISemanticTypeDecl
	{
		/// <summary>
		/// Deserialized name placeholder.
		/// </summary>
		public string OfTypeName { get; set; }

		/// <summary>
		/// The SemanticTypeStruct that defines the attributes of this decl.
		/// </summary>
		public ISemanticTypeStruct OfType { get; set; }

		/// <summary>
		/// The attribute values, which must match the OfType's native types.  
		/// TODO: Extend this to include ST's as well?
		/// </summary>
		public List<IAttributeValue> AttributeValues { get; set; }

		public SemanticTypeDecl()
		{
			AttributeValues = new List<IAttributeValue>();
		}
	}
}
