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
		List<INativeType> NativeTypes { get; }
		List<ISemanticElement> SemanticElements { get; }

		bool HasNativeTypes { get; }
		bool HasSemanticTypes { get; }
		bool HasChildTypes { get;}
	}

	public interface IAttributeValue
	{
		string Name { get; set; }
		string Value { get; set; }
	}

	public interface INativeType
	{
		string Name { get; }
		string ImplementingType { get; }
		object GetValue(object instance);
	}

	public interface ISemanticElement
	{
		string Name { get; }
		ISemanticType Element { get; set; }
	}

	/// <summary>
	/// Callback to create child types.
	/// </summary>
	public interface ISemanticTypeSystem
	{
		IRuntimeSemanticType Create(string typeName, IRuntimeSemanticType parent = null);
		ISemanticTypeStruct GetSemanticTypeStruct(string typeName);
	}
}
