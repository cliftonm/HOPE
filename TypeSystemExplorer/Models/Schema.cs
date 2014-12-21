/*
    Copyright 2104 Higher Order Programming

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using XTreeInterfaces;

/// *** DisplayName must be unique.  Annoyingly, the property grid change notifier doesn't give us the property name, it gives us the display name for the property being changed.

namespace TypeSystemExplorer.Models
{
	// References a semantic type.
	public class SubType : IHasCollection
	{
		[Category("References")]
		[XmlAttribute()]
		[TypeConverter(typeof(SemanticTypeNameConverter))]
		[DisplayName("Semantic Type")]
		public string Name { get; set; }

		[Category("Semantic Type")]
		[XmlAttribute()]
		[Description("Aliases are used for cosmetic purposes, such as column display names.  Parent aliases override child aliases.")]
		public string Alias { get; set; }

		[Category("Database")]
		[XmlAttribute()]
		[Description("Indicate whether this type is part of a unique composite key for the parent semantic element.")]
		public bool UniqueField { get; set; }

		[Category("UI")]
		[XmlAttribute()]
		[Description("Indicates the ordering of fields for display purposes.")]
		public int Ordinality { get; set; }

		[XmlIgnore]
		[Browsable(false)]
		public Dictionary<string, dynamic> Collection { get { return null; } }

		public SubType()
		{
		}
	}

	public class NativeType : IHasCollection
	{
		[Category("Native Type")]
		[XmlAttribute()]
		public string Name { get; set; }

		[Category("Native Type")]
		[XmlAttribute()]
		[Description("Aliases are used for cosmetic purposes, such as column display names.  Parent aliases override child aliases.")]
		public string Alias { get; set; }

		[Category("Native Type")]
		[XmlAttribute()]
		[DisplayName("Native Type")]
		[TypeConverter(typeof(ImplementingTypeNameConverter))]
		public string ImplementingType { get; set; }

		[Category("UI")]
		[XmlAttribute()]
		[Description("Indicates the ordering of fields for display purposes.  This is additive to the semantic element's ordinality, so that a semantic element with multiple native types can order the native types within the semantic element.")]
		public int Ordinality { get; set; }

		[Category("Database")]
		[XmlAttribute()]
		[Description("Indicate whether this field is part of a unique composite key for the parent semantic element.")]
		public bool UniqueField { get; set; }

		// Note that NT's do not have an ordinality.  This makes no sense because an NT is a high level abstracted property of a semantic type or sub-type.
		// It is the semantic sub-type that can have an ordinality.
		// The only use case for NT ordinality is if you have a root ST that is just composed of NT's, however this is most likely an indicator of a bad
		// hierarchy architecture, as a root ST most likely should be comprised of some level of more generalized ST's.
		// We may revisit this issue at some point.

		[XmlIgnore]
		[Browsable(false)]
		public Dictionary<string, dynamic> Collection { get {return null;}}

		public NativeType()
		{
		}
	}

	public class SemanticType : IHasCollection
	{
		[Category("Name")]
		[XmlAttribute()]
		public string Name { get; set; }

		[Category("Name")]
		[XmlAttribute()]
		[Description("Aliases are used for cosmetic purposes, such as column display names.  Parent aliases override child aliases.")]
		public string Alias { get; set; }

		[Category("Database")]
		[XmlAttribute()]
		[Description("Indicate whether this semantic element's fields are treated as a composite unique key.")]
		public bool UniqueField { get; set; }

		[XmlIgnore]
		[Browsable(false)]
		public Dictionary<string, dynamic> Collection { get; protected set; }

		// Serializable list.
		[Browsable(false)]
		public List<NativeType> NativeTypes { get; set; }
		[Browsable(false)]
		public List<SubType> SubTypes { get; set; }

		public SemanticType()
		{
			NativeTypes = new List<NativeType>();
			SubTypes = new List<SubType>();
			Collection = new Dictionary<string, dynamic>()
			{
				{"Models.NativeType", NativeTypes},
				{"Models.SubType", SubTypes},
			};
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
