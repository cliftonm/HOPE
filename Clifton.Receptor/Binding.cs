using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Clifton.Receptor
{
	public class Binding : ISupportInitialize
	{
		public string PropertyName { get; set; }
		public Control Control { get; set; }
		public object Source { get; set; }
		public string ControlPropertyName { get; set; }

		public void BeginInit()
		{
		}

		public void EndInit()
		{
			Control.DataBindings.Add(new System.Windows.Forms.Binding(ControlPropertyName, Source, PropertyName));
		}
	}
}
