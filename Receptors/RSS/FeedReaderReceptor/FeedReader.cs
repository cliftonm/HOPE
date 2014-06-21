using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clifton.ExtensionMethods;

using Clifton.Receptor.Interfaces;
using Clifton.SemanticTypeSystem.Interfaces;

namespace FeedReaderReceptor
{
	public class FeedReader : BaseReceptor
    {
		public override string Name { get { return "Feed Reader"; } }
		public override bool IsEdgeReceptor { get { return true; } }

		[UserConfigurableProperty("URL of feed:")]
		public string FeedUrl { get; set; }

		protected string title;

		public FeedReader(IReceptorSystem rsys)
			: base(rsys)
		{
		}

		public override void Initialize()
		{
			base.Initialize();

			// We need some database tables of we're going to persist and associate feeds and feed items with other data.
			RequireFeedTable();
			RequireFeedItemTable();
		}

		public override void EndInit()
		{
			base.EndInit();
			
			// Initialize the feed and if it's not in the database, create an instance so we get the feed's ID so we can associate that with our item output.
		}

		public override void ProcessCarrier(ICarrier carrier)
		{
			// Once the TTL is determined, the feed reader will add an event to the timer to remind itself to update the feed when the TTL expires.
		}

		protected void RequireFeedTable()
		{
			CreateCarrier("RequireTable", signal =>
				{
					signal.TableName = "RSSFeed";
					signal.Schema = "RSSFeed";
				});
			CreateCarrier("RequireTable", signal =>
			{
				signal.TableName = "RSSFeedItem";
				signal.Schema = "RSSFeedItem";
			});
		}

		protected void RequireFeedItemTable()
		{
		}
    }
}
