using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Clifton.Receptor
{
	public class BindingSource : ISupportInitialize
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
			System.Windows.Forms.BindingSource bs = new System.Windows.Forms.BindingSource();
			PropertyInfo pi = Source.GetType().GetProperty(PropertyName);
			object src = pi.GetValue(Source);
			bs.DataSource = src;
			pi = Control.GetType().GetProperty("DataSource");
			pi.SetValue(Control, bs);
		}
	}
}
