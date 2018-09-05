using System.Collections.Generic;

namespace MangaScraper.Application.Subscriptions {
    public class SubscriptionItem {
        public string Provider { get; set; }
        public string Url { get; set; }
        public HashSet<string> KnownChapters { get; set; }

        public string Path { get; set; }
    }
}
