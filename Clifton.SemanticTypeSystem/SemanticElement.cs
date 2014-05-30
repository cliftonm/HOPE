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
		public ISemanticType Element { get; set; }

		/// <summary>
		/// Resolve the ST down to it's singleton native type and return the value.
		/// </summary>
		public object GetValue(ISemanticTypeSystem sts, object instance)
		{
			object ret = null;

			PropertyInfo pi = instance.GetType().GetProperty(Name);
			Type type = pi.GetType();

			List<INativeType> ntypes = sts.GetSemanticTypeStruct(Name).NativeTypes;
			List<ISemanticElement> stypes = sts.GetSemanticTypeStruct(Name).SemanticElements;
			Assert.That(ntypes.Count + stypes.Count == 1, "Setting a value on a semantic type requires that the semantic type defines one and only one native type or child semantic type in order to resolve the native type property whose value is to be set.");
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
			PropertyInfo pi = instance.GetType().GetProperty(Name);
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
