using System;
using System.Collections.Generic;

using Clifton.SemanticTypeSystem.Interfaces;

namespace Clifton.SemanticTypeSystem
{
	/// <summary>
	/// A semantic type is a tuple of the declaration and the structure.
	/// </summary>
	public class SemanticType : ISemanticType
	{
		public ISemanticTypeDecl Decl { get; set; }
		public ISemanticTypeStruct Struct { get; set; }

		public SemanticType()
		{
		}
	}
}
