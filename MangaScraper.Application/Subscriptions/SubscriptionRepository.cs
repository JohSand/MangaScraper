using MangaScraper.Application.Services;
using MangaScraper.Core.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Utf8Json;
using static System.Environment;

namespace MangaScraper.Application.Subscriptions {
    public class SubscriptionRepository : ISubscriptionRepository {
        private readonly AsyncLock _fileLock = new AsyncLock();

        public SubscriptionRepository()
            : this(GetFolderPath(SpecialFolder.ApplicationData)) { }

        public SubscriptionRepository(string @base) {
            FilePath = Path.Combine(@base, "MangaScraper", "Subscriptions");
            if (!Directory.Exists(FilePath)) {
                Directory.CreateDirectory(FilePath);
            }
        }


        private string FilePath { get; }
        private Dictionary<(string, string), SubscriptionItem> _subscriptionItems;

        public async Task<SubscriptionItem> GetSubscription(string name, string provider) {
            if (_subscriptionItems == null)
                await ReadFromDisk();

            return _subscriptionItems.ContainsKey((name, provider)) ? _subscriptionItems[(name, provider)] : null;
        }

        public async Task<ICollection<SubscriptionItem>> GetSubscriptions() =>
            _subscriptionItems?.Values ?? await ReadFromDisk();

        private async Task<ICollection<SubscriptionItem>> ReadFromDisk() {
            FileInfo[] files;
            using (await _fileLock.LockAsync()) {
                files = new DirectoryInfo(FilePath).GetFiles("*.json");
            }

            var items = await files.Select(GetSubscription).WhenAll();
            _subscriptionItems = items.ToDictionary(i => (i.Name, i.Provider));
            return _subscriptionItems.Values;
        }

        private async Task<SubscriptionItem> GetSubscription(FileInfo subfile) {
            using (var fs = subfile.OpenRead()) {
                return await JsonSerializer.DeserializeAsync<SubscriptionItem>(fs);
            }
        }

        //public async Task Save(params SubscriptionItem[] subscriptionItems) => 
        //    await subscriptionItems.Select(item => SaveAsync(item)).WhenAll();

        public async Task Save(SubscriptionItem item) {
            if (_subscriptionItems.ContainsKey((item.Name, item.Provider)))
                _subscriptionItems[(item.Name, item.Provider)] = item;
            else
                _subscriptionItems.Add((item.Name, item.Provider), item);

            using(await _fileLock.LockAsync())
            using (var fs = File.Open(Path.Combine(FilePath, item.Name + ".json"), FileMode.Create)) {
                await JsonSerializer.SerializeAsync(fs, item);
            }
        }
    }
}
