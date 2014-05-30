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
