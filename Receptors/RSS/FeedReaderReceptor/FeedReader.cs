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

		protected SyndicationFeed feed;

		public FeedReader(IReceptorSystem rsys)
			: base(rsys)
		{
			AddEmitProtocol("RSSFeedItem");
		}

		/// <summary>
		/// If specified, immmediately acquire the feed and start emitting feed items.
		/// </summary>
		public override void EndSystemInit()
		{
			base.EndSystemInit();
			AcquireFeed();
		}

		/// <summary>
		/// When the user configuration fields have been updated, re-acquire the feed.
		/// </summary>
		public override void UserConfigurationUpdated()
		{
			base.UserConfigurationUpdated();
			AcquireFeed();
		}

		/// <summary>
		/// Acquire the feed and emit the feed items. 
		/// </summary>
		protected async void AcquireFeed()
		{
			if (!String.IsNullOrEmpty(FeedUrl))
			{
				try
				{
					SyndicationFeed feed = await GetFeedAsync(FeedUrl);
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
			SyndicationFeed feed = await Task.Run(() =>
				{
					XmlReader xr = XmlReader.Create(feedUrl);
					SyndicationFeed sfeed = SyndicationFeed.Load(xr);
					xr.Close();

					return sfeed;
				});

			return feed;
		}

		/// <summary>
		/// Emits only new feed items for display.
		/// </summary>
		protected void EmitFeedItems(SyndicationFeed feed)
		{
			feed.Items.ForEach(item =>
				{
					CreateCarrier("RSSFeedItem", signal =>
						{
							signal.FeedName = FeedName;
							signal.Title = item.Title.Text;
							signal.URL.Value = item.Links[0].Uri.ToString();
							signal.Description = item.Summary.Text;
							signal.Authors = String.Join(", ", item.Authors.Select(a => a.Name).ToArray());
							signal.Categories = String.Join(", ", item.Categories.Select(c => c.Name).ToArray());
							signal.PubDate = item.PublishDate.LocalDateTime;
						});
				});
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
