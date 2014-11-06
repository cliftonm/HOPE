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
		/// <summary>
		/// Placeholder name from XML deserialization.
		/// </summary>
		string DeclTypeName { get; }

		/// <summary>
		/// Beautified display name that replaces the fully qualified name.
		/// </summary>
		string Alias { get; }

		/// <summary>
		/// The ordering of elements in a semantic type.  Used for display purposes, like a grid view.
		/// </summary>
		int Ordinality { get; }

		/// <summary>
		/// Used by the semantic database to determine how this semantic type behaves with regards to duplicate data.
		/// </summary>
		bool Unique { get; }

		/// <summary>
		/// Any defined native types.
		/// </summary>
		List<INativeType> NativeTypes { get; }

		/// <summary>
		/// Any defined semantic elements.  This list is an intermediate list built during deserialization.
		/// Suggestion: Since the semantic element resolves to a semantic type, we could build the SemanticType list here as well.
		/// </summary>
		List<ISemanticElement> SemanticElements { get; }

		/// <summary>
		/// Return all types with the interface IGetSetSemanticType so we can get/set values of those types.
		/// </summary>
		List<IGetSetSemanticType> AllTypes { get; }

		bool HasNativeTypes { get; }
		bool HasSemanticTypes { get; }
		bool HasChildTypes { get; }

		/// <summary>
		/// Return the SE (possibly "this") that contains the specified SE.
		/// </summary>
		ISemanticTypeStruct SemanticElementContaining(ISemanticTypeStruct stsToFind);
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
		bool UniqueField { get; }
		string ImplementingType { get; }

		/// <summary>
		/// The ordering of elements in a semantic type.  Used for display purposes, like a grid view.
		/// </summary>
		int Ordinality { get; }

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

		/// <summary>
		/// Beautified display name that replaces the fully qualified name.
		/// </summary>
		string Alias { get; }

		/// <summary>
		/// Used by the semantic database to determine how this semantic type behaves with regards to duplicate data.
		/// </summary>
		bool UniqueField { get; set; }

		/// <summary>
		/// The ordering of elements in a semantic type.  Used for display purposes, like a grid view.
		/// </summary>
		int Ordinality { get; }

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

		/// <summary>
		/// Recurse into the named structure, returning itself and all sub-structures.
		/// The return is a list of tuples, where Item1 is the ST and Item2 is the parent ST of Item1
		/// </summary>
		List<Tuple<ISemanticTypeStruct, ISemanticTypeStruct>> GetAllSemanticTypes(string protocolName);

		List<IFullyQualifiedNativeType> GetFullyQualifiedNativeTypes(string protocolName, bool recurse = true);
		List<IFullyQualifiedNativeType> GetFullyQualifiedNativeTypeValues(dynamic signal, string protocolName, bool recurse = true);
		void SetFullyQualifiedNativeTypeValue(dynamic signal, string fqn, object val);
		bool VerifyProtocolExists(string protocol);

		void CreateCustomType(string typeName, List<string> childProtocols);
		bool TryGetSignalValue(dynamic signal, string semanticTypeName, out object val);
	}

	public interface IFullyQualifiedNativeType
	{
		string Name { get; }
		string Alias { get; }
		string FullyQualifiedName { get;}
		string FullyQualifiedNameSansRoot { get; }
		INativeType NativeType { get;}

		/// <summary>
		/// This is the aggregate of uniqueness -- if the parent ST is unique, then all native type fields deriving from it are unique.
		/// </summary>
		bool UniqueField { get; }

		/// <summary>
		/// The ordering of elements in a semantic type.  Used for display purposes, like a grid view.
		/// </summary>
		int Ordinality { get; }

		object Value { get; }
	}
}
