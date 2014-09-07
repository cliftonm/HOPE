using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using XTreeInterfaces;

namespace TypeSystemExplorer.Models
{
	public class SemanticType : IHasCollection
	{
		[Category("Name")]
		[XmlAttribute()]
		[TypeConverter(typeof(SemanticTypeNameConverter))]
		public string Name { get; set; }

		[XmlIgnore]
		[Browsable(false)]
		public Dictionary<string, dynamic> Collection { get { return null; } }

		public SemanticType()
		{
		}
	}

	public class SemanticTypesContainer : IHasCollection
	{
		[XmlAttribute()]
		public string Name { get; set; }

		[XmlIgnore]
		[Browsable(false)]
		public Dictionary<string, dynamic> Collection { get; protected set; }

		// Serializable list.
		[Browsable(false)]
		public List<SemanticType> SemanticTypes { get; set; }

		public SemanticTypesContainer()
		{
			SemanticTypes = new List<SemanticType>();
			Collection = new Dictionary<string, dynamic>() 
			{ 
				{"Models.SemanticType", SemanticTypes},
			};
		}
	}

	public class Schema : IHasCollection
	{
		[Category("Name")]
		[XmlAttribute()]
		public string Name { get; set; }

		[XmlIgnore]
		[Browsable(false)]
		public Dictionary<string, dynamic> Collection { get; protected set; }

		[Browsable(false)]
		public List<SemanticTypesContainer> SemanticTypesContainer { get; set; }

		[XmlIgnore]
		public static Schema Instance { get; protected set; }

		public Schema()
		{
			SemanticTypesContainer = new List<SemanticTypesContainer>();
			Collection = new Dictionary<string, dynamic>() 
			{
				{"Models.SemanticTypesContainer", SemanticTypesContainer},
			};

			Instance = this;
		}
	}
}
