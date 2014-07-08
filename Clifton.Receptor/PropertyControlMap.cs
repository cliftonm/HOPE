using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clifton.Receptor
{
	/// <summary>
	/// Support for declaratively describing mappings between user configurable properties ([UserConfigurableProperty("Feed URL:")]) and controls
	/// in declarative forms instantiated by MycroParser.  While this structure is rather simple at the moment, it may have additional properties
	/// and features at some point to further refine data binding, limits, ranges, text input options, etc.
	/// </summary>
	public class PropertyControlMap
	{
		protected List<PropertyControlEntry> entries;

		public List<PropertyControlEntry> Entries
		{
			get { return entries; }
			set { entries = value; }
		}

		public PropertyControlMap()
		{
			entries = new List<PropertyControlEntry>();
		}
	}

	public class PropertyControlEntry
	{
		public string PropertyName { get; set; }
		public string ControlName { get; set; }
		public string ControlPropertyName { get; set; }

		public PropertyControlEntry()
		{
		}
	}
}
