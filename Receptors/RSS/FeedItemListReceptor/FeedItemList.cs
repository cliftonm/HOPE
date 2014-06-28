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

namespace FeedItemListReceptor
{
	public class FeedItemList : BaseReceptor
    {
		public override string Name { get { return "Feed Item List"; } }
		public override bool IsEdgeReceptor { get { return true; } }

		protected Form form;
		protected DataTable dtItems;
		protected DataView dv;

		public FeedItemList(IReceptorSystem rsys)
			: base(rsys)
		{
			AddReceiveProtocol("RSSFeedItemDisplay", 
				// cast is required to resolve Func vs. Action in parameter list.
				(Action<dynamic>)(signal => AddFeedItem(signal)));

			AddReceiveProtocol("Recordset",
				signal => signal.Schema == "FeedItemPhrases",
				signal => ProcessFeedItems(signal));

			AddEmitProtocol("URL");

			InitializeViewer();
		}

		protected void AddFeedItem(dynamic signal)
		{
			DataRow row = dtItems.NewRow();
			row[0] = signal.FeedName;
			row[1] = signal.PubDate;
			row[2] = signal.Title;
			row[3] = signal.Categories;
			row[4] = signal.URL.Value;
			dtItems.Rows.Add(row);

			((DataGridView)form.Controls[0]).AutoResizeColumns();
		}

		protected void InitializeViewer()
		{
			form = MycroParser.InstantiateFromFile<Form>("feedItemList.xml", null);
			// Prevents mouse-over of the surface, which it becomes focused, to Z-move on top of the feed item list dialog.
			// TODO: This causes problems with other apps, as when the entire app loses focus to some other app.
			// form.TopMost = true;
			form.StartPosition = FormStartPosition.Manual;
			form.Location = new Point(0, 0);
			
			// Create a data table here.  There's probably a better place to do this.
			// TODO: The first two columns are suppposed to be read-only.
			dtItems = new DataTable();
			dtItems.Columns.Add(new DataColumn("Feed", typeof(string)));
			dtItems.Columns.Add(new DataColumn("Date", typeof(DateTime)));
			dtItems.Columns.Add(new DataColumn("Title", typeof(string)));
			dtItems.Columns.Add(new DataColumn("Categories", typeof(string)));
			dtItems.Columns.Add(new DataColumn("URL", typeof(string)));

			// Setup the data source.
			dv = new DataView(dtItems);
			((DataGridView)form.Controls[0]).DataSource = dv;
			((DataGridView)form.Controls[0]).Columns[4].Visible = false;
			((DataGridView)form.Controls[0]).CellContentDoubleClick += OnCellContentDoubleClick;

			form.Show();
		}

		protected void OnCellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			CreateCarrier("URL", signal => signal.Value = dv[e.RowIndex][4].ToString());
		}

		/// <summary>
		/// Respond to a feed item recordset, as we want to display these.
		/// </summary>
		protected void ProcessFeedItems(dynamic sig)
		{
			dtItems.Clear();
			List<dynamic> records = sig.Recordset;

			foreach (dynamic rec in records)
			{
				AddFeedItem(rec);
			}
		}
	}
}
