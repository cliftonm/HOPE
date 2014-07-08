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
		public override string ConfigurationUI { get { return "FeedReaderConfig.xml"; } }

		[UserConfigurableProperty("Feed URL:")]
		public string FeedUrl { get; set; }

		[UserConfigurableProperty("Feed Name:")]
		public string FeedName {get;set;}

		[UserConfigurableProperty("Feed Update Interval:")]
		public int RefreshInterval { get; set; }

		protected int feedID;
		protected SyndicationFeed feed;

		public FeedReader(IReceptorSystem rsys)
			: base(rsys)
		{
			AddEmitProtocol("RequireTable");
			AddEmitProtocol("DatabaseRecord");
			AddEmitProtocol("RSSFeedItemDisplay");
			AddEmitProtocol("URL");

			AddReceiveProtocol("IDReturn",
				signal => signal.TableName == "RSSFeed",
				signal =>
				{
					feedID = signal.ID;
					MarkOldFeedItems();
					SaveFeedItemsToDatabase(feed);
				});
			
			AddReceiveProtocol("IDReturn",
				signal => signal.TableName == "RSSFeedItem",
				signal =>
				{
					// Only emit signals regarding this feed item if it is truly a new feed item.
					if (signal.NewRow)
					{
						EmitFeedItemForDisplay(feed, signal.Tag);
						EmitFeedItemUrl(feed, signal.Tag);
					}
				});
		}

		public override void Initialize()
		{
			base.Initialize();

			// We need some database tables of we're going to persist and associate feeds and feed items with other data.
			RequireFeedTables();

			// Temporary:
//			FeedName = "Ars Technica";
//			FeedUrl = "http://feeds.arstechnica.com/arstechnica/index?format=xml";

			//FeedName = "NPR World News";
			//FeedUrl = "http://www.npr.org/rss/rss.php?id=1004";

//			FeedName = "Code Project Articles";
//			FeedUrl = "http://www.codeproject.com/WebServices/ArticleRSS.aspx";
		}

		public override async void EndSystemInit()
		{
			base.EndSystemInit();

			// If the receptor is disabled on load (Enabled==false) then the Initialize method
			// is never called.
			if (!String.IsNullOrEmpty(FeedUrl))
			{

				feed = await Task<SyndicationFeed>.Run(() =>
					{

						// =========== USE ACTUAL URL AS RSS FEED SOURCE ===============
						// The real version will create an XmlReader for the URL
						XmlReader xr = XmlReader.Create(FeedUrl);

						// =========== USE FILE AS RSS FEED SOURCE ===================
						// string data = File.ReadAllText("rss.xml");
						// TextReader tr = new StringReader(data);
						// XmlReader xr = XmlReader.Create(tr);

						SyndicationFeed sfeed = SyndicationFeed.Load(xr);
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

						return sfeed;

						// TODO: Once the TTL is determined, the feed reader will add an event to the timer to remind itself to update the feed when the TTL expires.
					});

				CreateMissingDatabaseFeedEntry(FeedName, FeedUrl, feed.Title.Text, feed.Description.Text);
			}
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
				signal.Row = CreateFeedRow(feedName, feedUrl, title, description);
				signal.UniqueKey = "URL";
				signal.Tag = "FeedReader";
			});
		}

		/// <summary>
		/// Saves each feed item to the DB
		/// </summary>
		protected void SaveFeedItemsToDatabase(SyndicationFeed feed)
		{
			foreach (SyndicationItem item in feed.Items)
			{
				CreateMissingDatabaseFeedItemEntry(
					feedID,
					item.Id,
					item.Title.Text,
					item.Links[0].Uri.ToString(),
					item.Summary.Text,
					String.Join(", ", item.Authors.Select(a => a.Name).ToArray()),
					String.Join(", ", item.Categories.Select(c => c.Name).ToArray()),
					item.PublishDate.LocalDateTime);
			}
		}

		/// <summary>
		/// Persists the feed item.
		/// FeedItemID IS NOT THE ID OF THE FEEDITEM record, it is the ID of the RSS feed item.
		/// </summary>
		protected void CreateMissingDatabaseFeedItemEntry(int rssFeedID, string feedItemID, string title, string url, string descr, string authors, string categories, DateTime pubDate)
		{
			CreateCarrierIfReceiver("DatabaseRecord", signal =>
			{
				signal.TableName = "RSSFeedItem";
				signal.Action = "InsertIfMissing";
				signal.Row = CreateFeedItemRow(rssFeedID, feedItemID, title, url, descr, authors, categories, pubDate);
				signal.UniqueKey = "FeedItemID";
				signal.Tag = feedItemID;
			});
		}

		protected ICarrier CreateFeedRow(string feedName, string feedUrl, string title, string description)
		{
			ISemanticTypeStruct rowProtocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("RSSFeed");
			dynamic rowSignal = rsys.SemanticTypeSystem.Create("RSSFeed");
			rowSignal.FeedName = feedName;
			rowSignal.URL.Value = feedUrl;
			rowSignal.Title = title;
			rowSignal.Description = description;
			ICarrier rowCarrier = rsys.CreateInternalCarrier(rowProtocol, rowSignal);

			return rowCarrier;
		}

		protected ICarrier CreateFeedItemRow(int rssFeedID, string feedItemID, string title, string url, string descr, string authors, string categories, DateTime pubDate)
		{
			ISemanticTypeStruct rowProtocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("RSSFeedItem");
			dynamic rowSignal = rsys.SemanticTypeSystem.Create("RSSFeedItem");
			rowSignal.RSSFeedID = rssFeedID;
			rowSignal.FeedItemID = feedItemID;
			rowSignal.Title = title;
			rowSignal.URL.Value = url;
			rowSignal.Description = descr;
			rowSignal.Authors = authors;
			rowSignal.Categories = categories;
			rowSignal.PubDate = pubDate;
			rowSignal.NewItem = true;
			rowSignal.ReadItem = false;
			ICarrier rowCarrier = rsys.CreateInternalCarrier(rowProtocol, rowSignal);

			return rowCarrier;
		}

		/// <summary>
		/// Emits only new feed items for display.
		/// </summary>
		protected void EmitFeedItemForDisplay(SyndicationFeed feed, string feedItemID)
		{
			SyndicationItem item = feed.Items.Single(i => i.Id == feedItemID);
			CreateCarrierIfReceiver("RSSFeedItemDisplay", signal =>
				{
					signal.FeedName = FeedName;
					signal.Title = item.Title.Text;
					signal.URL.Value = item.Links[0].Uri.ToString();
					signal.Description = item.Summary.Text;
					signal.Authors = String.Join(", ", item.Authors.Select(a => a.Name).ToArray());
					signal.Categories = String.Join(", ", item.Categories.Select(c => c.Name).ToArray());
					signal.PubDate = item.PublishDate.LocalDateTime;
				});
		}

		protected void EmitFeedItemUrl(SyndicationFeed feed, string feedItemID)
		{
			SyndicationItem item = feed.Items.Single(i => i.Id == feedItemID);
			// Anyone interested directly in the URL (like the NLP) can have a go at it right now.
			// The URL receptor, for example, would open the page on the browser.
			CreateCarrierIfReceiver("URL", signal => signal.Value = item.Links[0].Uri.ToString());

		}

		/// <summary>
		/// Mark all existing feed items for this feed ID as old.
		/// </summary>
		protected void MarkOldFeedItems()
		{
			ISemanticTypeStruct rowProtocol = rsys.SemanticTypeSystem.GetSemanticTypeStruct("RSSFeedNewItem");
			dynamic rowSignal = rsys.SemanticTypeSystem.Create("RSSFeedNewItem");
			rowSignal.NewItem = false;

			CreateCarrierIfReceiver("DatabaseRecord", signal =>
			{
				signal.TableName = "RSSFeedItem";
				signal.Action = "update";
				signal.Row = rsys.CreateInternalCarrier(rowProtocol, rowSignal);
				signal.Where = "RSSFeedID = " + feedID;
			});
		}
    }
}
