using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

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
		protected DataTable dtEntities;
		protected DataView dvEntities;
		protected DataTable dtKeywords;
		protected DataView dvKeywords;
		protected DataTable dtConcepts;
		protected DataView dvConcepts;

		protected DataGridView dgvEntities;
		protected DataGridView dgvKeywords;
		protected DataGridView dgvConcepts;

		protected Dictionary<string, int> resultTypeIDMap = new Dictionary<string, int>();

		public NlpViewer(IReceptorSystem rsys)
			: base(rsys)
		{
			InitializeViewer();
			AddEmitProtocol("DatabaseRecord");
			AddEmitProtocol("DropView");
			AddEmitProtocol("RequireView");

			AddReceiveProtocol("SearchDateRange",
				(Action<dynamic>)(signal =>
			{
				SearchDateRange(signal.BeginningDate, signal.EndingDate);
			}));

			// Don't forgot, explicit cast is required for some reason to differentiate between Action<dyanmic> and Func<bool, dynamic>
			AddReceiveProtocol("Recordset",
				signal => signal.Schema == "AlchemyPhrases" && signal.Tag == "Entities",
				signal => ProcessEntityPhrases(signal));

			AddReceiveProtocol("Recordset",
				signal => signal.Schema == "AlchemyPhrases" && signal.Tag == "Keywords",
				signal => ProcessKeywordPhrases(signal));

			AddReceiveProtocol("Recordset",
				signal => signal.Schema == "AlchemyPhrases" && signal.Tag == "Concepts",
				signal => ProcessConceptPhrases(signal));

			AddReceiveProtocol("Recordset",
				signal => signal.Schema == "AlchemyResultTypeRecord",
				signal =>
				{
					// Save our name to ID mapping of result types.
					foreach(dynamic row in signal.Records)
					{
						resultTypeIDMap[row.Name] = row.ID;
					}

					LoadPhrases();
				});
		}

		public override void Initialize()
		{
			base.Initialize();

			// If you need to change the view:
			CreateCarrier("DropView", signal =>
			{
				signal.ViewName = "AlchemyPhrases";
			});

			CreateCarrier("DropView", signal =>
			{
				signal.ViewName = "FeedItemPhrases";
			});

			CreateCarrier("RequireView", signal =>
				{
					signal.ViewName = "AlchemyPhrases";
					signal.Sql = "select ar.AlchemyPhraseID as AlchemyPhraseID, ar.AlchemyResultTypeID as AlchemyResultTypeID, et.Name as EntityName, ap.Name as Name, substr(ar.CaptureDate, 1, 10) as CaptureDate, count(distinct ar.RSSFeedItemID) as Count from AlchemyResult ar left join AlchemyPhrase ap on ar.AlchemyPhraseID = ap.ID left join AlchemyEntityType et on et.ID = ar.AlchemyEntityTypeID group by ar.AlchemyPhraseID, ar.AlchemyResultTypeID, et.Name, ap.Name, substr(ar.CaptureDate, 1, 10) order by count(distinct ar.RSSFeedItemID) desc";
				});

			CreateCarrier("RequireView", signal =>
			{
				signal.ViewName = "FeedItemPhrases";
				signal.Sql = "select distinct f.ID as RSSFeedItemID, ar.AlchemyPhraseID as AlchemyPhraseID, f.FeedName as FeedName, fi.NewItem as NewItem, fi.ReadItem as ReadItem, fi.PubDate as PubDate, fi.Title as Title, fi.Categories as Categories, fi.URL as URL, ar.CaptureDate as CaptureDate from AlchemyResult ar left join RSSFeedItem fi on fi.ID = ar.RSSFeedItemID left join RSSFeed f on f.ID = fi.RSSFeedID";
			});

			CreateCarrier("DatabaseRecord", signal =>
				{
					signal.Action = "select";
					signal.TableName = "AlchemyResultType";
					signal.ResponseProtocol = "AlchemyResultTypeRecord";
					signal.Tag = "NlpViewer";
				});
		}

		/// <summary>
		/// Load phrases for each result type.
		/// </summary>
		protected void LoadPhrases(string where = "")
		{
			RequestRecordset("Entity", "Entities", where);
			RequestRecordset("Keyword", "Keywords", where);
			RequestRecordset("Concept", "Concepts", where);
		}

		protected void RequestRecordset(string idName, string tagName, string where)
		{
			CreateCarrier("DatabaseRecord", signal =>
				{
					signal.Action = "select";				// only select is allowed on views.
					signal.ViewName = "AlchemyPhrases";
					signal.ResponseProtocol = "AlchemyPhrases";
					signal.Where = "AlchemyResultTypeID = " + resultTypeIDMap[idName];
					
					if (!String.IsNullOrEmpty(where))
					{
						signal.Where = signal.Where + " and " + where;
					}

					signal.Tag = tagName;
				});
		}

		protected void InitializeViewer()
		{
			MycroParser mp = new MycroParser();
			XmlDocument doc = new XmlDocument();
			doc.Load("NlpViewer.xml");
			mp.Load(doc, "Form", null);
			form = (Form)mp.Process();

			// form = MycroParser.InstantiateFromFile<Form>("NlpViewer.xml", null);
			dgvEntities = (DataGridView)mp.ObjectCollection["dgvEntities"];
			dgvKeywords = (DataGridView)mp.ObjectCollection["dgvKeywords"];
			dgvConcepts = (DataGridView)mp.ObjectCollection["dgvConcepts"];
			form.StartPosition = FormStartPosition.Manual;
			form.Location = new Point(100, 400);

			dtEntities = new DataTable();
			dtEntities.Columns.Add(new DataColumn("AlchemyPhraseID", typeof(int)));
			dtEntities.Columns.Add(new DataColumn("Name", typeof(string)));
			dtEntities.Columns.Add(new DataColumn("Type", typeof(string)));
			dtEntities.Columns.Add(new DataColumn("Count", typeof(int)));

			dvEntities = new DataView(dtEntities);

			dgvEntities.DataSource = dvEntities;
			dgvEntities.CellContentDoubleClick += OnCellContentDoubleClick;

			// ==============================

			dtConcepts = new DataTable();
			dtConcepts.Columns.Add(new DataColumn("AlchemyPhraseID", typeof(int)));
			dtConcepts.Columns.Add(new DataColumn("Name", typeof(string)));
			dtConcepts.Columns.Add(new DataColumn("Count", typeof(int)));

			dvConcepts = new DataView(dtConcepts);

			dgvConcepts.DataSource = dvConcepts;
			dgvConcepts.CellContentDoubleClick += OnCellContentDoubleClick;

			// ==============================

			dtKeywords = new DataTable();
			dtKeywords.Columns.Add(new DataColumn("AlchemyPhraseID", typeof(int)));
			dtKeywords.Columns.Add(new DataColumn("Name", typeof(string)));
			dtKeywords.Columns.Add(new DataColumn("Count", typeof(int)));

			dvKeywords = new DataView(dtKeywords);

			dgvKeywords.DataSource = dvKeywords;
			dgvKeywords.CellContentDoubleClick += OnCellContentDoubleClick;

			form.Show();
		}

		protected void ProcessEntityPhrases(dynamic signal)
		{
			List<dynamic> records = signal.Records;
			dtEntities.BeginLoadData();
			dtEntities.Clear();
			
			foreach (dynamic rec in records)
			{
				DataRow row = dtEntities.NewRow();
				row[0] = rec.AlchemyPhraseID;
				row[1] = rec.Name;
				row[2] = rec.EntityName;
				row[3] = rec.Count;
				dtEntities.Rows.Add(row);
			}

			dtEntities.EndLoadData();

			// http://connect.microsoft.com/VisualStudio/feedback/details/335552/datagridview-resets-first-column-to-visible-when-handle-is-not-yet-created
			// In my case, the issue is related to which tab has focus initially
			dgvEntities.Columns[0].Visible = false;
		}

		protected void ProcessKeywordPhrases(dynamic signal)
		{
			List<dynamic> records = signal.Records;
			dtKeywords.BeginLoadData();
			dtKeywords.Clear();

			foreach (dynamic rec in records)
			{
				DataRow row = dtKeywords.NewRow();
				row[0] = rec.AlchemyPhraseID;
				row[1] = rec.Name;
				row[2] = rec.Count;
				dtKeywords.Rows.Add(row);
			}

			dtKeywords.EndLoadData();

			// http://connect.microsoft.com/VisualStudio/feedback/details/335552/datagridview-resets-first-column-to-visible-when-handle-is-not-yet-created
			// In my case, the issue is related to which tab has focus initially
			dgvKeywords.Columns[0].Visible = false;
		}

		protected void ProcessConceptPhrases(dynamic signal)
		{
			List<dynamic> records = signal.Records;
			dtConcepts.BeginLoadData();
			dtConcepts.Clear();

			foreach (dynamic rec in records)
			{
				DataRow row = dtConcepts.NewRow();
				row[0] = rec.AlchemyPhraseID;
				row[1] = rec.Name;
				row[2] = rec.Count;
				dtConcepts.Rows.Add(row);
			}

			dtConcepts.EndLoadData();
			
			// http://connect.microsoft.com/VisualStudio/feedback/details/335552/datagridview-resets-first-column-to-visible-when-handle-is-not-yet-created
			// In my case, the issue is related to which tab has focus initially
			dgvConcepts.Columns[0].Visible = false;
		}

		protected void OnCellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			DataView dv = (DataView)((DataGridView)sender).DataSource;
			// A viewer will pick up the resulting FeedItemPhrasesRecordset.
			CreateCarrier("DatabaseRecord", signal =>
				{
					signal.Action = "select";				// only select is allowed on views.
					signal.ViewName = "FeedItemPhrases";
					signal.ResponseProtocol = "FeedItemPhrases";
					signal.Where = "AlchemyPhraseID = " + dv[e.RowIndex][0].ToString();
				});
		}

		protected void SearchDateRange(DateTime beginningDate, DateTime endingDate)
		{
			string begDate = beginningDate.ToString("yyyy-MM-dd HH:mm:ss");
			string endDate = endingDate.ToString("yyyy-MM-dd HH:mm:ss");
			string where = "CaptureDate between " + begDate.SingleQuote() + " and " + endDate.SingleQuote();
			LoadPhrases(where);
		}
    }
}
