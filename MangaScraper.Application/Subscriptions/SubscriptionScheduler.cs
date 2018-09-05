using MangaScraper.Core.Helpers;
using System.Threading.Tasks;

namespace MangaScraper.Application.Subscriptions {
    public class SubscriptionScheduler {
        private readonly SubscriptionService _subscriptionService;
        private readonly SubscriptionRepository _subscriptionRepository;

        public SubscriptionScheduler(SubscriptionService subscriptionService, SubscriptionRepository subscriptionRepository) {
            _subscriptionRepository = subscriptionRepository;
            _subscriptionService = subscriptionService;
        }

        public async Task Work() {
            var subscriptions = await _subscriptionRepository.GetSubscriptions();
            foreach (var sub in subscriptions) {
                var missingChapters = await _subscriptionService.DownloadMissingChapters(sub);

                missingChapters.ForEach(sub.KnownChapters.Add);
            }
            await _subscriptionRepository.Save();
        }
    }
}
