using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clifton.SemanticTypeSystem.Interfaces
{
	public interface IRuntimeSemanticType
	{
		void Initialize(ISemanticTypeSystem sts);
	}

	/// <summary>
	/// Concrete class is an ISemanticType.
	/// </summary>
	public interface ISemanticType
	{
		ISemanticTypeDecl Decl { get; set; }
		ISemanticTypeStruct Struct { get; set; }
	}

	public interface ISemanticTypeDecl
	{
		string OfTypeName { get; set; }
		ISemanticTypeStruct OfType { get; set; }
		List<IAttributeValue> AttributeValues { get; set; }
	}

	public interface ISemanticTypeStruct
	{
		string DeclTypeName { get; }
		string Alias { get; }
		bool Normalize { get; }
		List<INativeType> NativeTypes { get; }
		List<ISemanticElement> SemanticElements { get; }
		List<IGetSetSemanticType> AllTypes { get; }

		bool HasNativeTypes { get; }
		bool HasSemanticTypes { get; }
		bool HasChildTypes { get; }
	}

	public interface IAttributeValue
	{
		string Name { get; set; }
		string Value { get; set; }
	}

	public interface INativeType
	{
		string Name { get; }
		string Alias { get; }
		bool Normalize { get; }
		string ImplementingType { get; }

		// sts parameter is not used for the native type setter/getter, but provides compatability with ISemanticElement.SetValue so we can have a common interface.

		/// <summary>
		/// Returns the underlying native type of an element in a semantic structure, resolving sub-semenantic elements as well down to their native type.
		/// </summary>
		Type GetImplementingType(ISemanticTypeSystem sts);

		/// <summary>
		/// Resolve the ST down to it's singleton native type and return the value.
		/// </summary>
		object GetValue(ISemanticTypeSystem sts, object instance);

		/// <summary>
		/// Resolve the ST down to it's singleton native type and return the value.
		/// </summary>
		void SetValue(ISemanticTypeSystem sts, object instance, object value);
	}

	public interface IGetSetSemanticType
	{
		string Name { get; }

		// sts parameter is not used for the native type setter/getter, but provides compatability with ISemanticElement.SetValue so we can have a common interface.

		/// <summary>
		/// Returns the underlying native type of an element in a semantic structure, resolving sub-semenantic elements as well down to their native type.
		/// </summary>
		Type GetImplementingType(ISemanticTypeSystem sts);

		/// <summary>
		/// Resolve the ST down to it's singleton native type and return the value.
		/// </summary>
		object GetValue(ISemanticTypeSystem sts, object instance);

		/// <summary>
		/// Resolve the ST down to it's singleton native type and return the value.
		/// </summary>
		void SetValue(ISemanticTypeSystem sts, object instance, object value);
	}

	public interface ISemanticElement
	{
		string Name { get; }
		ISemanticType Element { get; set; }

		/// <summary>
		/// Returns the name of the native type or semantic element singleton implemented by the semantic element.
		/// This does NOT recurse to get the underlying native type from a semantic element hierarchy.  See GetImplementingType.
		/// </summary>
		string GetImplementingName(ISemanticTypeSystem sts);

		/// <summary>
		/// Returns the underlying native type of an element in a semantic structure, resolving sub-semenantic elements as well down to their native type.
		/// </summary>
		Type GetImplementingType(ISemanticTypeSystem sts);

		/// <summary>
		/// Resolve the ST down to it's singleton native type and return the value.
		/// </summary>
		object GetValue(ISemanticTypeSystem sts, object instance);

		/// <summary>
		/// Resolve the ST down to it's singleton native type and return the value.
		/// </summary>
		void SetValue(ISemanticTypeSystem sts, object instance, object value);
	}

	/// <summary>
	/// Callback to create child types.
	/// </summary>
	public interface ISemanticTypeSystem
	{
		IRuntimeSemanticType Create(string typeName, IRuntimeSemanticType parent = null);
		ISemanticTypeStruct GetSemanticTypeStruct(string typeName);
		Dictionary<string, ISemanticType> SemanticTypes { get;}
		dynamic Clone(dynamic sourceSignal, ISemanticElement se);
		List<IFullyQualifiedNativeType> GetFullyQualifiedNativeTypes(string protocolName);
		List<IFullyQualifiedNativeType> GetFullyQualifiedNativeTypeValues(dynamic signal, string protocolName);
		void SetFullyQualifiedNativeTypeValue(dynamic signal, string fqn, object val);
		bool VerifyProtocolExists(string protocol);
	}

	public interface IFullyQualifiedNativeType
	{
		string Name { get; }
		string Alias { get; }
		string FullyQualifiedName { get;}
		string FullyQualifiedNameSansRoot { get; }
		INativeType NativeType { get;}
		object Value { get; }
	}
}
