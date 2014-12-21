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
using System.Reflection;

using Clifton.Assertions;
using Clifton.SemanticTypeSystem.Interfaces;

namespace Clifton.SemanticTypeSystem
{
	/// <summary>
	/// A semantic element is a tuple containing both the declarative name and the concrete SemanticType instance.
	/// </summary>
	public class SemanticElement : ISemanticElement, IGetSetSemanticType
	{
		public string Name { get; set; }

		/// <summary>
		/// Beautified display name that replaces the fully qualified name.
		/// </summary>
		public string Alias { get; set; }

		/// <summary>
		/// Used by the semantic database to determine how this semantic type behaves with regards to duplicate data.
		/// </summary>
		public bool UniqueField { get; set; }

		public ISemanticType Element { get; set; }

		/// <summary>
		/// The ordering of elements in a semantic type.  Used for display purposes, like a grid view.
		/// </summary>
		public int Ordinality { get; set; }

		/// <summary>
		/// Returns the name of the native type or semantic element singleton implemented by the semantic element.
		/// This does NOT recurse to get the underlying native type from a semantic element hierarchy.  See GetImplementingType.
		/// </summary>
		public string GetImplementingName(ISemanticTypeSystem sts)
		{
			string ret = null;
			List<INativeType> ntypes = sts.GetSemanticTypeStruct(Name).NativeTypes;
			List<ISemanticElement> stypes = sts.GetSemanticTypeStruct(Name).SemanticElements;
			Assert.That(ntypes.Count + stypes.Count == 1, "Setting a value on a semantic type requires that the semantic type defines one and only one native type or child semantic type in order to resolve the native type property whose value is to be set.");

			if (ntypes.Count == 1)
			{
				ret = ntypes[0].Name;
			}
			else
			{
				ret = stypes[0].Name;
			}

			return ret;
		}

		/// <summary>
		/// Returns the underlying native type of an element in a semantic structure, resolving sub-semenantic elements as well down to their native type.
		/// </summary>
		public Type GetImplementingType(ISemanticTypeSystem sts)
		{
			Type ret = null;
			List<INativeType> ntypes = sts.GetSemanticTypeStruct(Name).NativeTypes;
			List<ISemanticElement> stypes = sts.GetSemanticTypeStruct(Name).SemanticElements;
			Assert.That(ntypes.Count + stypes.Count == 1, "Getting the element type of a semantic type requires that the semantic type defines one and only one native type or child semantic type in order to resolve the native type property whose value is to be set.");

			if (ntypes.Count == 1)
			{
				ret = ntypes[0].GetImplementingType(sts);
			}
			else
			{
				ret = stypes[0].GetImplementingType(sts);
			}

			return ret;
		}

		/// <summary>
		/// Resolve the ST down to it's singleton native type and return the value.
		/// </summary>
		public object GetValue(ISemanticTypeSystem sts, object instance)
		{
			object ret = null;

			PropertyInfo pi = instance.GetType().GetProperty(Name);

			List<INativeType> ntypes = sts.GetSemanticTypeStruct(Name).NativeTypes;
			List<ISemanticElement> stypes = sts.GetSemanticTypeStruct(Name).SemanticElements;
			Assert.That(ntypes.Count + stypes.Count == 1, "Getting a value on a semantic type requires that the semantic type defines one and only one native type or child semantic type in order to resolve the native type property whose value is to be set.");
			object item = pi.GetValue(instance);

			if (ntypes.Count == 1)
			{
				PropertyInfo piTarget = item.GetType().GetProperty(ntypes[0].Name);
				ret = piTarget.GetValue(item);
			}
			else
			{
				ret = stypes[0].GetValue(sts, item);
			}

			return ret;
		}

		/// <summary>
		/// Resolve the ST down to it's singleton native type and return the value.
		/// </summary>
		public void SetValue(ISemanticTypeSystem sts, object instance, object value)
		{
			// Don't set the value if it's null from the DB.
			if (value != DBNull.Value)
			{
				PropertyInfo pi = instance.GetType().GetProperty(Name);

				if (pi == null)
				{
					// The instance IS the native type that is wrapped by the semantic type.
					List<INativeType> ntypes = sts.GetSemanticTypeStruct(Name).NativeTypes;
					Assert.That(ntypes.Count == 1, "Setting a value on a semantic type requires that the semantic type defines one and only one native type or child semantic type in order to resolve the native type property whose value is to be set.");
					PropertyInfo piTarget = instance.GetType().GetProperty(ntypes[0].Name);
					piTarget.SetValue(instance, value);
				}
				else
				{
					Type type = pi.GetType();

					List<INativeType> ntypes = sts.GetSemanticTypeStruct(Name).NativeTypes;
					List<ISemanticElement> stypes = sts.GetSemanticTypeStruct(Name).SemanticElements;
					Assert.That(ntypes.Count + stypes.Count == 1, "Setting a value on a semantic type requires that the semantic type defines one and only one native type or child semantic type in order to resolve the native type property whose value is to be set.");
					object item = pi.GetValue(instance);

					if (ntypes.Count == 1)
					{
						PropertyInfo piTarget = item.GetType().GetProperty(ntypes[0].Name);
						piTarget.SetValue(item, value);
					}
					else
					{
						stypes[0].SetValue(sts, item, value);
					}
				}
			}
		}
	}
}
											  
