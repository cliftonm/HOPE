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
		[Flags]
		protected enum ItemStates
		{
			New = 0x01,
			Displayed = 0x02,
			Visited = 0x04
		}

		protected Color visitedColor = Color.FromArgb(0x98, 0xFB, 0x98);		// Pale Green for visited.
		protected Color displayedColor = Color.FromArgb(0x87, 0xCE, 0xFA);		// Light Sky Blue for "old feed".

		public override string Name { get { return "Feed Item List"; } }
		public override bool IsEdgeReceptor { get { return true; } }
		public override string ConfigurationUI { get { return null; } }

		protected Dictionary<string, ItemStates> rowStateByUrl;

		public FeedItemList(IReceptorSystem rsys)
			: base(rsys, "feedItemList.xml")
		{
			// The only protocol we receive.
			AddReceiveProtocol("RSSFeedItem", (Action<dynamic>)(signal => ShowSignal(signal)));
			AddEmitProtocol("ExceptionMessage");
			AddEmitProtocol("UrlVisited");
			AddEmitProtocol("RSSFeedItemDisplayed");

			rowStateByUrl = new Dictionary<string, ItemStates>();
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

			// Override the carrier list viewer's setting
			dgvSignals.AlternatingRowsDefaultCellStyle.BackColor = Color.Empty;

			// Hook the cell formatting event so we can color the rows on the fly, which
			// compensates for when the user sorts by a column.
			dgvSignals.CellFormatting += OnCellFormatting;
		}

		// TODO: The problem with this is, once a new feed item is set to "displayed", when the user sorts the data,
		// the grid will now display the new feed as an old feed, and the only thing the user did was change the sort order!
		protected void OnCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
			// Only once per row.
			if (e.ColumnIndex == 0)
			{
				ItemStates state;

				// Gnarly.  Nasty.  Yuck.  TODO: What can we do to fix all these hardcoded fully qualified NT paths?
				if (rowStateByUrl.TryGetValue(dgvSignals.Rows[e.RowIndex].Cells["RSSFeedItem.RSSFeedUrl.Url.Value"].Value.ToString(), out state))
				{
					if ((state & ItemStates.Visited) == ItemStates.Visited)
					{
						dgvSignals.Rows[e.RowIndex].DefaultCellStyle.BackColor = visitedColor;
					}
					else if ((state & ItemStates.Displayed) == ItemStates.Displayed)
					{
						dgvSignals.Rows[e.RowIndex].DefaultCellStyle.BackColor = displayedColor;
					}
				}
			}
		}

		protected override void CreateViewerTable()
		{
			base.CreateViewerTable();

			// This doesn't work:
			// dgvSignals.Sort(dgvSignals.Columns["RSSFeedItem.RSSFeedUrl.Url.Value"], System.ComponentModel.ListSortDirection.Descending);

			// And neither does this:
			// dvSignals.Sort = "RSSFeedItem.RSSFeedUrl.Url.Value desc";

			// I also tried putting these into ProcessCarrier after the call to base.ProcessCarrier.
			// WTF?

			// And neither does this:
			// dgvSignals.Columns["RSSFeedItem.RSSFeedUrl.Url.Value"].SortMode = DataGridViewColumnSortMode.Programmatic;
			// dgvSignals.Sort(dgvSignals.Columns["RSSFeedItem.RSSFeedUrl.Url.Value"], System.ComponentModel.ListSortDirection.Descending);

			// The real solution of course is to have the query determine the sorting.  Very annoying though that I haven't figured out how to do this programmatically!
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

			if (carrier.Protocol.DeclTypeName == "RSSFeedItem")
			{
				ShowSignal(carrier.Signal);

				// We are interested in the existence of the root and whether an UrlVisited ST exists on it.
				// We determine the following:

				// RSSFeedItem with no parent: this is a new feed coming off the FeedReader itself
				// RSSFeedItem with parent: this is an existing feed (which may also be duplicated from FeedReader, we have no way of knowing the order)
				//		If UrlVisited exists, mark it as visited
				//		If UrlVisited is null, mark it as "old but no visited"
				// Otherwise, any RSSFeedItems with no parent an no (even null) RSSFeedVisted are marked as actually being new!

				// We potentially have a race condition:
				// A new feed (not seen) is stored to the DB
				// The query occurs, and while the UrlVisited is null, it's now viewed as "old"

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

				// This url value is the same as val.RSSFeedUrl.Url.Value -- we know this because this is what is being joined on in the composite ST.
				// Therefore, it's simpler to match on url in the code below.
				string url = carrier.Signal.RSSFeedUrl.Url.Value;

				if (carrier.ParentCarrier != null)
				{
					dynamic val;

					// Visited takes precedence over displayed.
					// If it's visited, of course it's been displayed.
					if (rsys.SemanticTypeSystem.TryGetSignalValue(carrier.ParentCarrier.Signal, "UrlVisited", out val))
					{
						foreach (DataGridViewRow row in dgvSignals.Rows)
						{
							if (row.Cells["RSSFeedItem.RSSFeedUrl.Url.Value"].Value.ToString() == url)
							{
								row.DefaultCellStyle.BackColor = visitedColor;
								rowStateByUrl[url] = ItemStates.Visited;
								break;
							}
						}
					}
					else   // not visited, however, possibly displayed previously.
					{
						// Do we have an RSSFeedItemDisplayed ST?
						if (rsys.SemanticTypeSystem.TryGetSignalValue(carrier.ParentCarrier.Signal, "RSSFeedItemDisplayed", out val))
						{
							// Find the row and set the background color to a light blue to indicate "old feed item"
							foreach (DataGridViewRow row in dgvSignals.Rows)
							{
								if (row.Cells["RSSFeedItem.RSSFeedUrl.Url.Value"].Value.ToString() == url)
								{
									row.DefaultCellStyle.BackColor = displayedColor;
									rowStateByUrl[url] = ItemStates.Displayed;
									break;
								}
							}
						}
						else
						{
							// This record has not been seen before.
							// Emit the "ItemDisplayed" ST for this URL but keep our display state as new until the DB is re-queried.
							CreateCarrierIfReceiver("RSSFeedItemDisplayed", signal => signal.RSSFeedUrl.Url.Value = url, false);
							rowStateByUrl[url] = ItemStates.New;
						}
					}
				}
				else
				{
					// No parent carrier, the feed is possibly coming from the feed reader directly.  Regardless, try marking that the feed has been displayed.
					// However, keep our display state as "new" until we receive something from the database indicating that we've displayed this feed item before.
					CreateCarrierIfReceiver("RSSFeedItemDisplayed", signal => signal.RSSFeedUrl.Url.Value = url, false);

					// Only update if the row state is not currently set, as we don't want to reset displayed or visited to the "new" state.
					if (!rowStateByUrl.ContainsKey(url))
					{
						rowStateByUrl[url] = ItemStates.New;
					}
				}
			}
		}

		// When the user double-clicks on a value, we post the RSSFeedVisted carrier with the URL.
		protected override void OnCellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			base.OnCellContentDoubleClick(sender, e);
			dgvSignals.Rows[e.RowIndex].DefaultCellStyle.BackColor = visitedColor;
			string url = dgvSignals.Rows[e.RowIndex].Cells["RSSFeedItem.RSSFeedUrl.Url.Value"].Value.ToString();
			rowStateByUrl[url] = ItemStates.Visited;
			CreateCarrierIfReceiver("UrlVisited", signal => signal.Url.Value = url, false);
		}
	}
}

/*
void dgv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
{
// Example - Row formatting
if (e.ColumnIndex == 0) // so this only runs once per row formatting
{
DataGridViewRow row = dgv.Rows[e.RowIndex];
if (row.Cells["some_cell"].Value.ToString() == "A")
row.DefaultCellStyle.BackColor = Color.Red;
return;

}
}
*/