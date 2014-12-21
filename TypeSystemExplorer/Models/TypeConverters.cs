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

namespace TypeSystemExplorer.Models
{
	public class ImplementingTypeNameConverter : StringConverter
	{
		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		{
			return true;
		}

		public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			List<string> names = new List<string>() { "char", "string", "bool", "int", "long", "float", "double", "decimal", "DateTime", "Object"};
			// Schema.Instance.SemanticTypesContainer.ForEach(t => t.SemanticTypes.ForEach(a => names.Add(a.Name)));
			names.Sort();

			return new StandardValuesCollection(names);
		}
	}

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
