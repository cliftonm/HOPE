using System;
using System.Collections.Generic;
using System.Reflection;

namespace Clifton.SemanticTypeSystem
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

		/// <summary>
		/// Using reflection, returns the value of a native type for the specified instance.
		/// </summary>
		public object GetValue(object instance)
		{
			PropertyInfo pi = instance.GetType().GetProperty(Name);

			return pi.GetValue(instance);
		}
	}
}
