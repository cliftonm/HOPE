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

using Clifton.SemanticTypeSystem.Interfaces;
using Clifton.Tools.Data;

namespace Clifton.SemanticTypeSystem
{
	/// <summary>
	/// A native type is used to hold a value in the underlying language's intrinsic type system.
	/// For example: "string foobar;"
	/// </summary>
	public class NativeType : INativeType, IGetSetSemanticType
	{
		/// <summary>
		/// Resolves to the name of the field/property.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Beautified display name that replaces the fully qualified name.
		/// </summary>
		public string Alias { get; set; }

		/// <summary>
		/// The ordering of elements in a semantic type.  Used for display purposes, like a grid view.
		/// </summary>
		public int Ordinality { get; set; }

		/// <summary>
		/// Used by the semantic database to determine how this semantic type behaves with regards to duplicate data.
		/// </summary>
		public bool UniqueField { get; set; }

		/// <summary>
		/// Resolves to the intrinsic type.
		/// </summary>
		public string ImplementingType { get; set; }

		// TODO: We have "ImplementingType" property and "GetImplementingType" method.  UGLY!
		public Type GetImplementingType(ISemanticTypeSystem sts)
		{
			switch (ImplementingType.ToLower())
			{
				case "string":
					return typeof(string);
				case "int":
				case "integer":
					return typeof(int);
				case "uint":
					return typeof(uint);
				case "short":
					return typeof(short);
				case "ushort":
					return typeof(ushort);
				case "long":
					return typeof(long);
				case "ulong":
					return typeof(ulong);
				case "float":
					return typeof(float);
				case "double":
					return typeof(double);
				case "decimal":
					return typeof(decimal);
				case "bool":
				case "boolean":
					return typeof(bool);
				case "datetime":
					// TODO: Should be an ST made up of date/time, which themselves are ST's of day, month, year, hour, minute second?
					return typeof(DateTime);
				case "byte":
					return typeof(byte);
				case "sbyte":
					return typeof(sbyte);
				case "char":
					return typeof(char);
				case "object":
					return typeof(object);
				default:
					// throw new ApplicationException("Unknown implementing type: " + ImplementingType);
					return null;
			}
		}

		/// <summary>
		/// Returns the value of a native type for the specified dynamic instance.
		/// </summary>
		public object GetValue(ISemanticTypeSystem sts, object instance)
		{
			PropertyInfo pi = instance.GetType().GetProperty(Name);

			return pi.GetValue(instance);
		}

		/// <summary>
		/// Set the native type property of the dynamic instance to the specified value.
		/// </summary>
		public void SetValue(ISemanticTypeSystem sts, object instance, object value)
		{
			Type type = instance.GetType();
			PropertyInfo pi = type.GetProperty(Name);
			object val = Converter.Convert(value, pi.PropertyType);
			pi.SetValue(instance, val);
		}
	}
}
