using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Utf8Json;

namespace MangaScraper.Application.Subscriptions {
    public class SubscriptionRepository {
        public SubscriptionRepository() => 
            Base = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        public SubscriptionRepository(string @base) => 
            Base = @base;

        private string Base { get; }
        private string FilePath => Path.Combine(Base, "MangaScraper", "subscriptions.json");
        private List<SubscriptionItem> _subscriptionItems;

        public async Task<IEnumerable<SubscriptionItem>> GetSubscriptions() {
            if (_subscriptionItems != null)
                return _subscriptionItems;

            using (var fs = File.Open(FilePath, FileMode.Create)) {
                return _subscriptionItems = await JsonSerializer.DeserializeAsync<List<SubscriptionItem>>(fs);
            }
        }

        public async Task Save() {
            using (var fs = File.Open(FilePath, FileMode.Create)) {
                await JsonSerializer.SerializeAsync(fs, _subscriptionItems ?? new List<SubscriptionItem>(0));
            }
        }
    }
}
