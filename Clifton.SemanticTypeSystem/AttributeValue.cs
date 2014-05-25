using System;
using System.Collections.Generic;

namespace Clifton.SemanticTypeSystem
{
	/// <summary>
	/// An attribute value.  These are instantiations of the SemanticTypeStruct underling the OfType SemanticTypeDecl,
	/// consisting of name-value pairs.
	/// </summary>
	public class AttributeValue
	{
		public string Name { get; set; }
		public string Value { get; set; }
	}
}
