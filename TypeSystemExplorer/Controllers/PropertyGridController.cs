using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clifton.SemanticTypeSystem;

using TypeSystemExplorer.Views;

namespace TypeSystemExplorer.Controllers
{
	public class PropertyGridController : ViewController<PropertyGridView>
	{
		protected void Opening()
		{
		}

		protected void Closing()
		{
		}

		public void ShowObject(object obj)
		{
			if (obj is KeyValuePair<string, SemanticType>)
			{
				View.ShowObject(((KeyValuePair<string, SemanticType>)obj).Value);
			}
			else
			{
				View.ShowObject(obj);
			}
		}
	}
}
