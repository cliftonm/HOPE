using System;
using System.Collections.Generic;

using Clifton.SemanticTypeSystem.Interfaces;

namespace Clifton.SemanticTypeSystem
{
	/// <summary>
	/// The structure of the semantic type.
	/// </summary>
	public class SemanticTypeStruct : ISemanticTypeStruct
	{
		/// <summary>
		/// Placeholder name from XML deserialization.
		/// </summary>
		public string DeclTypeName { get; set; }
		/// <summary>
		/// The SemanticTypeDecl instance.
		/// </summary>
		public SemanticTypeDecl DeclType { get; set; }
		/// <summary>
		/// Any defined native types.
		/// </summary>
		public List<NativeType> NativeTypes { get; set; }
		/// <summary>
		/// Any defined semantic elements.  This list is an intermediate list built during deserialization.
		/// Suggestion: Since the semantic element resolves to a semantic type, we could build the SemanticType list here as well.
		/// </summary>
		public List<SemanticElement> SemanticElements { get; set; }

		public bool HasNativeTypes { get { return NativeTypes.Count > 0; } }
		public bool HasSemanticTypes { get { return SemanticElements.Count > 0; } }
		public bool HasChildTypes { get { return HasNativeTypes || HasSemanticTypes; } }

		public SemanticTypeStruct()
		{
			NativeTypes = new List<NativeType>();
			SemanticElements = new List<SemanticElement>();
		}
	}
}
