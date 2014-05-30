using System;
using System.Collections.Generic;

using Clifton.SemanticTypeSystem.Interfaces;

namespace Clifton.SemanticTypeSystem
{
	/// <summary>
	/// A semantic element is a tuple containing both the declarative name and the concrete SemanticType instance.
	/// </summary>
	public class SemanticElement : ISemanticElement
	{
		public string Name { get; set; }
		public ISemanticType Element { get; set; }
	}
}
