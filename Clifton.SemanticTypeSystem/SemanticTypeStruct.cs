using System;
using System.Collections.Generic;
using System.Linq;

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
		/// Beautified display name that replaces the fully qualified name.
		/// </summary>
		public string Alias { get; set; }

		/// <summary>
		/// Used by the semantic database to determine how this semantic type behaves with regards to duplicate data.
		/// </summary>
		public bool Unique { get; set; }

		/// <summary>
		/// The ordering of elements in a semantic type.  Used for display purposes, like a grid view.
		/// </summary>
		public int Ordinality { get; set; }
		
		/// <summary>
		/// The SemanticTypeDecl instance.
		/// </summary>
		public SemanticTypeDecl DeclType { get; set; }
		
		/// <summary>
		/// Any defined native types.
		/// </summary>
		public List<INativeType> NativeTypes { get; set; }

		/// <summary>
		/// Any defined semantic elements.  This list is an intermediate list built during deserialization.
		/// Suggestion: Since the semantic element resolves to a semantic type, we could build the SemanticType list here as well.
		/// </summary>
		public List<ISemanticElement> SemanticElements { get; set; }

		/// <summary>
		/// Return all types with the interface IGetSetSemanticType so we can get/set values of those types.
		/// </summary>
		public List<IGetSetSemanticType> AllTypes { get { return NativeTypes.Cast<IGetSetSemanticType>().Concat(SemanticElements.Cast<IGetSetSemanticType>()).ToList(); } }

		public bool HasNativeTypes { get { return NativeTypes.Count > 0; } }
		public bool HasSemanticTypes { get { return SemanticElements.Count > 0; } }
		public bool HasChildTypes { get { return HasNativeTypes || HasSemanticTypes; } }

		public SemanticTypeStruct()
		{
			NativeTypes = new List<INativeType>();
			SemanticElements = new List<ISemanticElement>();
		}

		/// <summary>
		/// Return the SE (possibly "this") that contains the specified SE.
		/// </summary>
		public ISemanticTypeStruct SemanticElementContaining(ISemanticTypeStruct stsToFind)
		{
			ISemanticTypeStruct sts = null;

			foreach (SemanticElement se in SemanticElements)
			{
				if (se.Element.Struct == stsToFind)
				{
					sts = se.Element.Struct;
					break;
				}

				// Recurse.
				sts = se.Element.Struct.SemanticElementContaining(stsToFind);

				if (sts != null)
				{
					break;
				}
			}

			return sts;
		}

	}
}
