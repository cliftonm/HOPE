using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using Clifton.ExtensionMethods;
using Clifton.Tools.Strings.Extensions;

using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

namespace FeedReaderReceptor
{
	public class FeedReader : BaseReceptor
    {
		public override string Name { get { return "Feed Reader"; } }
		public override bool IsEdgeReceptor { get { return true; } }

		[UserConfigurableProperty("Feed URL:")]
		public string FeedUrl { get; set; }

		[UserConfigurableProperty("Feed Name:")]
		public string FeedName {get;set;}

		protected string feedID;
		protected SyndicationFeed feed;

		public FeedReader(IReceptorSystem rsys)
			: base(rsys)
		{
			AddEmitProtocol("RequireTable");

			AddReceiveProtocol("GetIDRecordset",
				signal => feedID = signal.Recordset[0].ID);
		}

		public override void Initialize()
		{
			base.Initialize();

			// We need some database tables of we're going to persist and associate feeds and feed items with other data.
			RequireFeedTables();

			// Temporary:
			FeedName = "Ars Technica";
			FeedUrl = "http://feeds.arstechnica.com/arstechnica/index?format=xml";

			EndInit();		// for testing
		}

		public override void EndInit()
		{
			base.EndInit();
			
			// =========== USE ACTUAL URL AS RSS FEED SOURCE ===============
			// The real version will create an XmlReader for the URL
			// XmlReader xr = XmlReader.Create(FeedUrl);

			// =========== USE FILE AS RSS FEED SOURCE ===================
			string data = File.ReadAllText("rss.xml");
			TextReader tr = new StringReader(data);
			XmlReader xr = XmlReader.Create(tr);

			feed = SyndicationFeed.Load(xr);
			xr.Close();

			// ============ CREATE A FILE FROM AN RSS FEED ===================
			/*
			XmlDocument xdoc = new XmlDocument();
			xdoc.Load(FeedUrl);
			StringBuilder sb = new StringBuilder();
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			XmlWriter xw = XmlWriter.Create(sb, settings);
			xdoc.WriteTo(xw);
			xw.Close();
			File.WriteAllText("rss.xml", sb.ToString());
			*/

			// TODO: Once the TTL is determined, the feed reader will add an event to the timer to remind itself to update the feed when the TTL expires.

			CreateMissingDatabaseFeedEntry(FeedName, FeedUrl, feed.Title.Text, feed.Description.Text);
			GetFeedID();
			ProcessFeedItems(feed);
		}

		protected void RequireFeedTables()
		{
			CreateCarrierIfReceiver("RequireTable", signal =>
				{
					signal.TableName = "RSSFeed";
					signal.Schema = "RSSFeed";
				});

			CreateCarrierIfReceiver("RequireTable", signal =>
			{
				signal.TableName = "RSSFeedItem";
				signal.Schema = "RSSFeedItem";
			});
		}

		/// <summary>
		/// Requests that the database record is created if it doesn't already exist.
		/// </summary>
		protected void CreateMissingDatabaseFeedEntry(string feedName, string feedUrl, string title, string description)
		{
			CreateCarrierIfReceiver("DatabaseRecord", signal =>
				{
					signal.TableName = "RSSFeed";
					signal.Action = "InsertIfMissing";
					signal.Row = CreateRow(feedName, feedUrl, title, description);
					signal.UniqueKey = "URL";
				});
		}

		protected void GetFeedID()
		{
			CreateCarrierIfReceiver("DatabaseRecord", signal =>
				{
					signal.TableName = "RSSFeed";
					signal.Where = "FeedName = " + FeedName.SingleQuote();
					signal.ResponseProtocol = "GetID";
					signal.Action = "select";
				});
		}

		protected ICarrier CreateRow(string feedName, string feedUrl, string title, string description)
		{
			// Create the type for the updated data.
			ISemanticTypeStruct rowProtocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("RSSFeed");
			dynamic rowSignal = rsys.SemanticTypeSystem.Create("RSSFeed");
			rowSignal.FeedName = feedName;
			rowSignal.URL.Value = feedUrl;
			rowSignal.Title = title;
			rowSignal.Description = description;
			ICarrier rowCarrier = rsys.CreateInternalCarrier(rowProtocol, rowSignal);

			return rowCarrier;
		}

		protected void ProcessFeedItems(SyndicationFeed feed)
		{
			foreach (SyndicationItem item in feed.Items)
			{
			}
		}
    }
}
