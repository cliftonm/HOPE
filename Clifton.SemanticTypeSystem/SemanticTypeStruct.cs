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

using Clifton.SemanticTypeSystem.Interfaces;

namespace Clifton.SemanticTypeSystem
{
	/// <summary>
	/// The structure of the semantic type.
	/// </summary>
	public class SemanticTypeStruct : ISemanticTypeStruct
	{
		/// <summary>
		/// Placeholder name from XML deserialization.
		/// </summary>
		public string DeclTypeName { get; set; }

		/// <summary>
		/// Beautified display name that replaces the fully qualified name.
		/// </summary>
		public string Alias { get; set; }

		/// <summary>
		/// Used by the semantic database to determine how this semantic type behaves with regards to duplicate data.
		/// </summary>
		public bool Unique { get; set; }

		/// <summary>
		/// The ordering of elements in a semantic type.  Used for display purposes, like a grid view.
		/// </summary>
		public int Ordinality { get; set; }
		
		/// <summary>
		/// The SemanticTypeDecl instance.
		/// </summary>
		public SemanticTypeDecl DeclType { get; set; }
		
		/// <summary>
		/// Any defined native types.
		/// </summary>
		public List<INativeType> NativeTypes { get; set; }

		/// <summary>
		/// Any defined semantic elements.  This list is an intermediate list built during deserialization.
		/// Suggestion: Since the semantic element resolves to a semantic type, we could build the SemanticType list here as well.
		/// </summary>
		public List<ISemanticElement> SemanticElements { get; set; }

		/// <summary>
		/// Return all types with the interface IGetSetSemanticType so we can get/set values of those types.
		/// </summary>
		public List<IGetSetSemanticType> AllTypes { get { return NativeTypes.Cast<IGetSetSemanticType>().Concat(SemanticElements.Cast<IGetSetSemanticType>()).ToList(); } }

		public bool HasNativeTypes { get { return NativeTypes.Count > 0; } }
		public bool HasSemanticTypes { get { return SemanticElements.Count > 0; } }
		public bool HasChildTypes { get { return HasNativeTypes || HasSemanticTypes; } }

		public SemanticTypeStruct()
		{
			NativeTypes = new List<INativeType>();
			SemanticElements = new List<ISemanticElement>();
		}

		/// <summary>
		/// Return the SE (possibly "this") that contains the specified SE.
		/// </summary>
		public ISemanticTypeStruct SemanticElementContaining(ISemanticTypeStruct stsToFind)
		{
			ISemanticTypeStruct sts = null;

			foreach (SemanticElement se in SemanticElements)
			{
				if (se.Element.Struct == stsToFind)
				{
					sts = se.Element.Struct;
					break;
				}

				// Recurse.
				sts = se.Element.Struct.SemanticElementContaining(stsToFind);

				if (sts != null)
				{
					break;
				}
			}

			return sts;
		}

		/// <summary>
		/// Flatten the hierarchy of semantic elements.
		/// </summary>
		public List<ISemanticTypeStruct> FlattenedSemanticTypes()
		{
			List<ISemanticTypeStruct> ret = new List<ISemanticTypeStruct>();
			ret.Add(this);

			foreach (ISemanticElement se in SemanticElements)
			{
				FlattenedSemanticTypes(ret, se);
			}

			return ret;
		}

		/// <summary>
		/// Internal recursion to flatten the hierarcy of semantic elements.
		/// </summary>
		protected void FlattenedSemanticTypes(List<ISemanticTypeStruct> ret, ISemanticElement se)
		{
			ret.Add(se.Element.Struct);

			foreach (ISemanticElement s in se.Element.Struct.SemanticElements)
			{
				FlattenedSemanticTypes(ret, se);
			}
		}
	}
}
