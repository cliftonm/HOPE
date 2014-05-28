using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clifton.SemanticTypeSystem.Interfaces
{
	/// <summary>
	/// Concrete class is an ISemanticType.
	/// </summary>
	public interface ISemanticType
	{
		void Initialize(ISemanticTypeSystem sts);
	}

	public interface ISemanticTypeStruct
	{
		string DeclTypeName { get; }
		List<INativeType> NativeTypes { get; }
	}

	public interface INativeType
	{
		string Name { get; }
		string ImplementingType { get; }
		object GetValue(object instance);
	}

	/// <summary>
	/// Callback to create child types.
	/// </summary>
	public interface ISemanticTypeSystem
	{
		ISemanticType Create(string typeName, ISemanticType parent = null);
		ISemanticTypeStruct GetSemanticTypeStruct(string typeName);
	}
}
