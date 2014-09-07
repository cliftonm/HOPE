using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeSystemExplorer.Models
{
	public class SemanticTypeNameConverter : StringConverter
	{
		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		{
			return true;
		}

		public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			List<string> names = new List<string>();
			Schema.Instance.SemanticTypesContainer.ForEach(t => t.SemanticTypes.ForEach(a => names.Add(a.Name)));
			names.Sort();

			return new StandardValuesCollection(names);
		}
	}
}
