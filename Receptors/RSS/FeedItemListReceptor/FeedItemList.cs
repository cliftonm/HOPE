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
			AddEmitProtocol("RSSFeedVisited");
			AddEmitProtocol("RSSFeedItemDisplayed");
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

		public override void ProcessCarrier(ICarrier carrier)
		{
			base.ProcessCarrier(carrier);
			ShowSignal(carrier.Signal);

			// We are interested in the existence of the root and whether an RSSFeedVisited ST exists on it.
			// We determine the following:

			// RSSFeedItem with no parent: this is a new feed coming off the FeedReader itself
			// RSSFeedItem with parent: this is an existing feed (which may also be duplicated from FeedReader, we have no way of knowing the order)
			//		If RSSFeedVisited exists, mark it as visited
			//		If RSSFeedVistied is null, mark it as "old but no visited"
			// Otherwise, any RSSFeedItems with no parent an no (even null) RSSFeedVisted are marked as actually being new!
			
			// We potentially have a race condition:
			// A new feed (not seen) is stored to the DB
			// The query occurs, and while the RSSFeedVisited is null, it's now viewed as "old"

			// HOWEVER: THE ABOVE IS INCORRECT!!!

			// I'm leaving the above comments for now so one can see how to think "wrongly" about architecture.
			// The reason the above is wrong is that we're determining feed "old" state from information that has nothing to do with managing 
			// whether the feed has been seen or not.
			// Instead, we actually need a type, something like "seen before" to tell us the state.  After all, the feed reader may be persisting
			// data without a viewer, or we just have a viewer without the feed reader being present.  Or we have a feed reader and viewer, but no
			// database.  The architecture MUST be flexible to handle these different scenarios.
			
			// Another interesting point is that the "seen before" state is not a flag, it's actually a semantic type!  This is a vitally important
			// distinction to make with regards to semantic systems -- they manage state semantically rather than with some non-semantic boolean that
			// just happens to be lableled "seen before".  It's going to be hard to convince people that this is a better way, because in all reality,
			// we have no real use cases to say it's better other than to say, look, you can determine state semantically rather than by querying a field
			// within a record.  What advantage does that have?  Well, it's a semantic state, but that isn't necessarily convincing enough.
			
			// Anyways, this explains why we have an RSSFeedItemDisplayed ST, so we know whether the feed viewer has ever displayed this feed before.
		}

		// TODO: When the user double-clicks on a value, we post the RSSFeedVisted carrier with the URL.
	}
}
