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

namespace NlpViewerReceptor
{
	public class NlpViewer : BaseReceptor
    {
		public override string Name { get { return "NLP Viewer"; } }
		public override bool IsEdgeReceptor { get { return true; } }

		protected Form form;
		protected DataTable dtItems;
		protected DataView dv;

		public NlpViewer(IReceptorSystem rsys)
			: base(rsys)
		{
			InitializeViewer();
			AddEmitProtocol("DatabaseRecord");
			AddEmitProtocol("DropView");
			AddEmitProtocol("RequireView");

			// Don't forgot, explicit cast is required for some reason to differentiate between Action<dyanmic> and Func<bool, dynamic>
			AddReceiveProtocol("AlchemyPhrasesRecordset", (Action<dynamic>)(signal => ProcessPhrases(signal)));
		}

		public override void Initialize()
		{
			base.Initialize();

			// If you need to change the view:
			//CreateCarrier("DropView", signal =>
			//{
			//	signal.ViewName = "AlchemyPhrases";
			//});

			CreateCarrier("DropView", signal =>
			{
				signal.ViewName = "FeedItemPhrases";
			});

			CreateCarrier("RequireView", signal =>
				{
					signal.ViewName = "AlchemyPhrases";
					signal.Sql = "select ar.PhraseID as PhraseID, ap.Name as Name, count(ar.PhraseID) as Count from AlchemyResult ar left join AlchemyPhrase ap on ar.PhraseID = ap.ID group by ar.PhraseID, ap.Name order by count(PhraseID) desc";
				});

			CreateCarrier("RequireView", signal =>
			{
				signal.ViewName = "FeedItemPhrases";
				signal.Sql = "select f.ID as FeedItemID, ar.PhraseID as PhraseID, f.FeedName as FeedName, fi.PubDate as PubDate, fi.Title as Title, fi.Categories as Categories, fi.URL as URL from AlchemyResult ar left join RSSFeedItem fi on fi.ID = ar.FeedItemID left join RSSFeed f on f.ID = fi.RSSFeedID";
			});

			CreateCarrier("DatabaseRecord", signal =>
				{
					signal.Action = "select";				// only select is allowed on views.
					signal.ViewName = "AlchemyPhrases";
					signal.ResponseProtocol = "AlchemyPhrases";
				});
		}

		protected void InitializeViewer()
		{
			form = MycroParser.InstantiateFromFile<Form>("NlpViewer.xml", null);
			form.StartPosition = FormStartPosition.Manual;
			form.Location = new Point(0, 400);

			dtItems = new DataTable();
			dtItems.Columns.Add(new DataColumn("PhraseID", typeof(int)));
			dtItems.Columns.Add(new DataColumn("Name", typeof(string)));
			dtItems.Columns.Add(new DataColumn("Count", typeof(int)));

			dv = new DataView(dtItems);

			((DataGridView)form.Controls[0]).DataSource = dv;
			((DataGridView)form.Controls[0]).Columns[0].Visible = false;
			((DataGridView)form.Controls[0]).CellContentDoubleClick += OnCellContentDoubleClick;

			form.Show();
		}

		protected void ProcessPhrases(dynamic signal)
		{
			List<dynamic> records = signal.Recordset;
			dtItems.BeginLoadData();
			
			foreach (dynamic rec in records)
			{
				DataRow row = dtItems.NewRow();
				row[0] = rec.PhraseID;
				row[1] = rec.Name;
				row[2] = rec.Count;
				dtItems.Rows.Add(row);
			}

			dtItems.EndLoadData();
		}

		protected void OnCellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			// A viewer will pick up the resulting FeedItemPhrasesRecordset.
			CreateCarrier("DatabaseRecord", signal =>
				{
					signal.Action = "select";				// only select is allowed on views.
					signal.ViewName = "FeedItemPhrases";
					signal.ResponseProtocol = "FeedItemPhrases";
					signal.Where = "PhraseID = " + dv[e.RowIndex][0].ToString();
				});
		}
    }
}
