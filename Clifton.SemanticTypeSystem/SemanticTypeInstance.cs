using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clifton.SemanticTypeSystem.Interfaces;

namespace Clifton.SemanticTypeSystem
{
	/// <summary>
	/// A container for an instance of the semantic type, including:
	/// The instance name
	/// The instance itself
	/// Any parent
	/// The Guid key used to locate the entry in the dictionary of instances.
	/// </summary>
	public class SemanticTypeInstance
	{
		public string Name { get; set; }
		public ISemanticType Instance { get; set; }
		public ISemanticType Parent { get; set; }
		public Guid Key { get; set; }
		public SemanticType Definition { get; set; }
	}
}
