using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Clifton.Receptor.Interfaces;

namespace ReceptorUiHelpers
{
	public interface ISupportModelessConfiguration
	{
		bool Modeless { get; }
	}

    public static class Helper
    {
		public static List<string> PopulateProtocolComboBox(ComboBox cbProtocols, IReceptorSystem rsys, string protocolName)
		{
			List<string> types = rsys.SemanticTypeSystem.SemanticTypes.Keys.ToList();
			types.Sort();
			cbProtocols.DataSource = types;

			if (!String.IsNullOrEmpty(protocolName))
			{
				cbProtocols.SelectedItem = protocolName;
			}

			return types;
		}
	}
}
