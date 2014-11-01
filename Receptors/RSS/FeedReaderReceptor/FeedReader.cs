// #define SIMULATED

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
		public override string Subname { get { return FeedName; } }
		public override bool IsEdgeReceptor { get { return true; } }
		public override string ConfigurationUI { get { return "FeedReaderConfig.xml"; } }

		[UserConfigurableProperty("Feed URL:")]
		public string FeedUrl { get; set; }

		[UserConfigurableProperty("Feed Name:")]
		public string FeedName {get;set;}

		protected string lastFeedUrl;
		protected SyndicationFeed feed;

		public FeedReader(IReceptorSystem rsys)
			: base(rsys)
		{
			AddReceiveProtocol("RSSFeedUrl", (Action<dynamic>)(s => ProcessUrl(s)));
			AddReceiveProtocol("RSSFeedRefresh", (Action<dynamic>)(s => AcquireFeed(lastFeedUrl)));
			AddEmitProtocol("RSSFeedItem");
			AddEmitProtocol("ExceptionMessage");
			AddEmitProtocol("LoggerMessage");
		}

		/// <summary>
		/// If specified, immmediately acquire the feed and start emitting feed items.
		/// </summary>
		public override void EndSystemInit()
		{
			base.EndSystemInit();

			if (Enabled)
			{
				AcquireFeed(FeedUrl);
			}
		}

		/// <summary>
		/// When the user configuration fields have been updated, re-acquire the feed.
		/// </summary>
		public override bool UserConfigurationUpdated()
		{
			base.UserConfigurationUpdated();

			if (Enabled)
			{
				AcquireFeed(FeedUrl);
			}

			return true;
		}

		protected void ProcessUrl(dynamic signal)
		{
			string feedUrl = signal.Url.Value;
			AcquireFeed(feedUrl);
		}

		/// <summary>
		/// Acquire the feed and emit the feed items. 
		/// </summary>
		protected async void AcquireFeed(string feedUrl)
		{
			if (!String.IsNullOrEmpty(feedUrl))
			{
				lastFeedUrl = feedUrl;

				try
				{
					SyndicationFeed feed = await GetFeedAsync(feedUrl);
					EmitFeedItems(feed);
				}
				catch (Exception ex)
				{
					EmitException(ex);
				}
			}
		}

		/// <summary>
		/// Acquire the feed asynchronously.
		/// </summary>
		protected async Task<SyndicationFeed> GetFeedAsync(string feedUrl)
		{
#if SIMULATED
			return null;
#else
			CreateCarrier("LoggerMessage", signal => 
				{
					signal.TextMessage.Text.Value = "Acquiring feed " + feedUrl + ".";
					signal.MessageTime = DateTime.Now;
				});

			SyndicationFeed feed = await Task.Run(() =>
				{
					// To handle this error:
					// For security reasons DTD is prohibited in this XML document. To enable DTD processing set the DtdProcessing property on XmlReaderSettings to Parse and pass the settings into XmlReader.Create method.
					XmlReaderSettings settings = new XmlReaderSettings();
					settings.XmlResolver = null;
					settings.DtdProcessing = DtdProcessing.Ignore;

					XmlReader xr = XmlReader.Create(feedUrl);
					SyndicationFeed sfeed = SyndicationFeed.Load(xr);
					xr.Close();

					return sfeed;
				});

			CreateCarrier("LoggerMessage", signal =>
				{
					signal.TextMessage.Text.Value = "Feed " + feedUrl + " has " + feed.Items.Count().ToString() + " items.";
					signal.MessageTime = DateTime.Now;
				});

			return feed;
#endif
		}

		/// <summary>
		/// Emits only new feed items for display.
		/// </summary>
		protected void EmitFeedItems(SyndicationFeed feed, int maxItems = Int32.MaxValue)
		{
#if SIMULATED
			CreateCarrier("RSSFeedItem", signal =>
				{
					signal.RSSFeedName.Name.Text.Value = FeedName;
					signal.RSSFeedTitle.Title.Text.Value = "Test Title";
					signal.RSSFeedUrl.Url.Value = "http://test";
					signal.RSSFeedDescription.Description.Text.Value = "Test Description";
					signal.RSSFeedAuthors.Text.Value = "Marc";
					signal.RSSFeedCategories.Text.Value = "My Stuff";
					signal.RSSFeedPubDate.Value = new DateTime(2014, 8, 19, 12, 1, 0);		// use a fixed date to test semantic database.
				});
#else
			// Allow -1 to also represent max items.
			int max = (maxItems == -1 ? feed.Items.Count() : maxItems);
			max = Math.Min(max, feed.Items.Count());		// Which ever is less.

			feed.Items.ForEachWithIndexOrUntil((item, idx) =>
				{
					CreateCarrier("RSSFeedItem", signal =>
						{
							signal.RSSFeedName.Name.Text.Value = FeedName;
							signal.RSSFeedTitle.Title.Text.Value = item.Title.Text;
							signal.RSSFeedUrl.Url.Value = item.Links[0].Uri.ToString();
							signal.RSSFeedDescription.Description.Text.Value = item.Summary.Text;
							
							// TODO: The semantic database doesn't support collections at the moment.
							signal.RSSFeedAuthors.Text.Value = String.Join(", ", item.Authors.Select(a => a.Name));
							signal.RSSFeedCategories.Text.Value = String.Join(", ", item.Categories.Select(c => c.Name));
							// Should be instead:
							// TODO: More semantically correct, Author and Category should be their own ST.
//							signal.RSSFeedAuthors.Value = new List<string>(item.Authors.Select(a => a.Name));
//							signal.RSSFeedCategories.Value = new List<string>(item.Categories.Select(c => c.Name));

							signal.RSSFeedPubDate.Value = item.PublishDate.LocalDateTime;
						});
				}, ((item, idx) => idx >= max));
#endif
		}
/*
		protected void EmitFeedItemUrl(SyndicationFeed feed, string feedItemID)
		{
			SyndicationItem item = feed.Items.Single(i => i.Id == feedItemID);
			// Anyone interested directly in the URL (like the NLP) can have a go at it right now.
			// The URL receptor, for example, would open the page on the browser.
			CreateCarrierIfReceiver("URL", signal => signal.Value = item.Links[0].Uri.ToString());
		}
*/
    }
}
