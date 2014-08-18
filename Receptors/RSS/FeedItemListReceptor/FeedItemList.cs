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
		protected DataGridView dgv;

		public FeedItemList(IReceptorSystem rsys)
			: base(rsys)
		{
			AddEmitProtocol("URL");
			AddEmitProtocol("DropView");
			AddEmitProtocol("RequireView");
			AddEmitProtocol("DatabaseRecord");

			AddReceiveProtocol("RSSFeedItemDisplay", 
				// cast is required to resolve Func vs. Action in parameter list.
				(Action<dynamic>)(signal => AddFeedItem(signal)));

			AddReceiveProtocol("Recordset",
				signal => signal.Schema == "FeedItemPhrases",
				signal => ProcessFeedItems(signal));

			AddReceiveProtocol("Recordset",
				signal => signal.Schema == "RSSFeedItemDisplay",
				signal => ProcessFeedItems(signal));

			AddReceiveProtocol("SearchDateRange",
				(Action<dynamic>)(signal =>
				{
					SearchDateRange(signal.BeginningDate, signal.EndingDate);
				}));

			InitializeViewer();
		}

		public override void Initialize()
		{
			base.Initialize();

			// If you need to change the view:
			CreateCarrier("DropView", signal =>
			{
				signal.ViewName = "FeedItems";
			});

			CreateCarrier("RequireView", signal =>
			{
				signal.ViewName = "FeedItems";
				signal.Sql = "select distinct f.ID as RSSFeedItemID, f.FeedName as FeedName, fi.Authors as Authors, fi.Description as Description, fi.NewItem as NewItem, fi.ReadItem as ReadItem, fi.PubDate as PubDate, fi.Title as Title, fi.Categories as Categories, fi.URL as URL from RSSFeedItem fi left join RSSFeed f on f.ID = fi.RSSFeedID";
			});

		}

		public override void Terminate()
		{
			base.Terminate();

			form.Close();
		}

		protected void AddFeedItem(dynamic signal)
		{
			DataRow row = dtItems.NewRow();
			row[0] = signal.FeedName;
			row[1] = signal.PubDate;
			row[2] = signal.Title;
			row[3] = signal.Categories;
			row[4] = signal.URL.Value;
			row[5] = signal.NewItem;
			row[6] = signal.ReadItem;
			dtItems.Rows.Add(row);
		}

		protected void InitializeViewer()
		{
			form = MycroParser.InstantiateFromFile<Form>("feedItemList.xml", null);
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
			dtItems.Columns.Add(new DataColumn("NewItem", typeof(string)));
			dtItems.Columns.Add(new DataColumn("ReadItem", typeof(string)));

			// Setup the data source.
			dv = new DataView(dtItems);
			dgv = (DataGridView)form.Controls[0];
			dgv.DataSource = dv;
			dgv.Columns[4].Visible = false;
			dgv.Columns[5].Visible = false;
			dgv.Columns[6].Visible = false;
			dgv.CellContentDoubleClick += OnCellContentDoubleClick;

			form.Show();
		}

		protected void OnCellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			CreateCarrier("URL", signal => signal.Value = dv[e.RowIndex][4].ToString());
			dv[e.RowIndex][6] = true;
			dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
		}

		/// <summary>
		/// Respond to a feed item recordset, as we want to display these.
		/// We're taking full advantage of duck-typing here, as sig will come from different instances
		/// but always define the fields we want.
		/// </summary>
		protected void ProcessFeedItems(dynamic sig)
		{
			dtItems.Clear();
			List<dynamic> records = sig.Records;
			dtItems.BeginLoadData();

			foreach (dynamic rec in records)
			{
				AddFeedItem(rec);
			}

			dtItems.EndLoadData();

			foreach (DataGridViewRow row in dgv.Rows)
			{
				bool newRow = Convert.ToBoolean(row.Cells[5].Value);	// Is the row new?
				bool readRow = Convert.ToBoolean(row.Cells[6].Value);	// Has the row been read?

				if (readRow)
				{
					row.DefaultCellStyle.BackColor = Color.White;
				}
				else
				{
					if (newRow)
					{
						row.DefaultCellStyle.BackColor = Color.LightBlue;
					}
					else
					{
						row.DefaultCellStyle.BackColor = Color.LightGreen;
					}
				}
			}

			((DataGridView)form.Controls[0]).AutoResizeColumns();
		}

		protected void SearchDateRange(DateTime beginningDate, DateTime endingDate)
		{
			string begDate = beginningDate.ToString("yyyy-MM-dd HH:mm:ss");
			string endDate = endingDate.ToString("yyyy-MM-dd HH:mm:ss");
			string where = "PubDate between " + begDate.SingleQuote() + " and " + endDate.SingleQuote();

			CreateCarrier("DatabaseRecord", signal =>
			{
				signal.Action = "select";				// only select is allowed on views.
				signal.ViewName = "FeedItems";
				signal.ResponseProtocol = "RSSFeedItemDisplay";
				signal.Where = where;
				signal.OrderBy = "PubDate desc";
			});
		}
	}
}
