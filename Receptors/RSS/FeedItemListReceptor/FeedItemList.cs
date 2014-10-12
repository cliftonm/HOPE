using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Clifton.ExtensionMethods;
using Clifton.MycroParser;
using Clifton.Tools.Strings.Extensions;

using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

using CarrierListViewerReceptor;

namespace FeedItemListReceptor
{
	/// <summary>
	/// The CarrierListViewer provides most of the functionality that we want.
	/// </summary>
	public class FeedItemList : CarrierListViewer
    {
		public override string Name { get { return "Feed Item List"; } }
		public override bool IsEdgeReceptor { get { return true; } }
		public override string ConfigurationUI { get { return null; } }

		public FeedItemList(IReceptorSystem rsys)
			: base(rsys, "feedItemList.xml")
		{
			// The only protocol we receive.
			AddReceiveProtocol("RSSFeedItem", (Action<dynamic>)(signal => ShowSignal(signal)));
			AddEmitProtocol("ExceptionMessage");
		}

		public override void EndSystemInit()
		{
			base.EndSystemInit();
			ProtocolName = "RSSFeedItem";
			UserConfigurationUpdated();
		}

		protected override void InitializeUI()
		{
			base.InitializeUI();
			dgvSignals.AlternatingRowsDefaultCellStyle.BackColor = Color.Empty;
		}

		/// <summary>
		/// We want to stop the base class behavior here.
		/// </summary>
		protected override void ListenForProtocol()
		{
			ISemanticTypeStruct st = rsys.SemanticTypeSystem.GetSemanticTypeStruct(ProtocolName);
			st.SemanticElements.ForEach(se => AddEmitProtocol(se.Name));
		}
	}
}
