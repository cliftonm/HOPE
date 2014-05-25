using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clifton.SemanticTypeSystem
{
	/// <summary>
	/// A semantic type declaration.
	/// </summary>
	public class SemanticTypeDecl
	{
		/// <summary>
		/// Deserialized name placeholder.
		/// </summary>
		public string OfTypeName { get; set; }

		/// <summary>
		/// The SemanticTypeStruct that defines the attributes of this decl.
		/// </summary>
		public SemanticTypeStruct OfType { get; set; }

		/// <summary>
		/// The attribute values, which must match the OfType's native types.  
		/// TODO: Extend this to include ST's as well?
		/// </summary>
		public List<AttributeValue> AttributeValues { get; set; }

		public SemanticTypeDecl()
		{
			AttributeValues = new List<AttributeValue>();
		}
	}
}
