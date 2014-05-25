using System;
using System.Collections.Generic;

namespace Clifton.SemanticTypeSystem
{
	/// <summary>
	/// A semantic type is a tuple of the declaration and the structure.
	/// </summary>
	public class SemanticType
	{
		public SemanticTypeDecl Decl { get; set; }
		public SemanticTypeStruct Struct { get; set; }

		public SemanticType()
		{
		}
	}
}
