using System;
using System.Collections.Generic;

namespace Clifton.SemanticTypeSystem
{
	/// <summary>
	/// A semantic element is a tuple containing both the declarative name and the concrete SemanticType instance.
	/// </summary>
	public class SemanticElement
	{
		public string Name { get; set; }
		public SemanticType Element { get; set; }
	}
}
