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
				default:
					throw new ApplicationException("Unknown implementing type: " + ImplementingType);
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
