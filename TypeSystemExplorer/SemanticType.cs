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

namespace TypeSystemExplorer
{
	/// <summary>
	/// A native type is used to hold a value in the underlying language's intrinsic type system.
	/// For example: "string foobar;"
	/// </summary>
	public class NativeType
	{
		/// <summary>
		/// Resolves to the name of the field/property.
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Resolves to the intrinsic type.
		/// </summary>
		public string ImplementingType { get; set; }
	}

	/// <summary>
	/// A semantic element is a tuple containing both the declarative name and the concrete SemanticType instance.
	/// </summary>
	public class SemanticElement
	{
		public string Name { get; set; }
		public SemanticType Element { get; set; }
	}

	/// <summary>
	/// The structure of the semantic type.
	/// </summary>
	public class SemanticTypeStruct
	{
		/// <summary>
		/// Placeholder name from XML deserialization.
		/// </summary>
		public string DeclTypeName { get; set; }
		/// <summary>
		/// The SemanticTypeDecl instance.
		/// </summary>
		public SemanticTypeDecl DeclType { get; set; }
		/// <summary>
		/// Any defined native types.
		/// </summary>
		public List<NativeType> NativeTypes { get; set; }
		/// <summary>
		/// Any defined semantic elements.  This list is an intermediate list built during deserialization.
		/// Suggestion: Since the semantic element resolves to a semantic type, we could build the SemanticType list here as well.
		/// </summary>
		public List<SemanticElement> SemanticElements { get; set; }

		public SemanticTypeStruct()
		{
			NativeTypes = new List<NativeType>();
			SemanticElements = new List<SemanticElement>();
		}
	}

	/// <summary>
	/// An attribute value.  These are instantiations of the SemanticTypeStruct underling the OfType SemanticTypeDecl,
	/// consisting of name-value pairs.
	/// </summary>
	public class AttributeValue
	{
		public string Name { get; set; }
		public string Value { get; set; }
	}

	/// <summary>
	/// A semantic type declaration.
	/// </summary>
	public class SemanticTypeDecl
	{
		/// <summary>
		/// Deserialized name placeholder.
		/// </summary>
		public string OfTypeName { get; set; }

		/// <summary>
		/// The SemanticTypeStruct that defines the attributes of this decl.
		/// </summary>
		public SemanticTypeStruct OfType { get; set; }

		/// <summary>
		/// The attribute values, which must match the OfType's native types.  
		/// TODO: Extend this to include ST's as well?
		/// </summary>
		public List<AttributeValue> AttributeValues { get; set; }

		public SemanticTypeDecl()
		{
			AttributeValues = new List<AttributeValue>();
		}
	}

	/// <summary>
	/// A semantic type is a tuple of the declaration and the structure.
	/// </summary>
	public class SemanticType
	{
		public SemanticTypeDecl Decl { get; set; }
		public SemanticTypeStruct Struct { get; set; }

		public SemanticType()
		{
		}
	}
}
