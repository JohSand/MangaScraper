using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MangaScraper.Application.Subscriptions {
    public class SubscriptionScheduler {
        private readonly SubscriptionService _subscriptionService;
        private readonly SubscriptionRepository _subscriptionRepository;

        public SubscriptionScheduler(SubscriptionService subscriptionService, SubscriptionRepository subscriptionRepository) {
            _subscriptionRepository = subscriptionRepository;
            _subscriptionService = subscriptionService;
        }

        public Task Start(string parser, CancellationToken token) {
            var scheduler = new SingleThreadTaskScheduler(ApartmentState.MTA);

            async Task EventLoop() {
                using (scheduler) {
                    while (!token.IsCancellationRequested) {
                        await Work();
                        await Task.Delay(600 * 1000, token);
                    }
                }
            }

            return Task.Factory.StartNew(
                EventLoop,
                token,
                TaskCreationOptions.None,
                scheduler
              )
              .Unwrap();
        }


        public async Task Work() {
            var subscriptions = await _subscriptionRepository.GetSubscriptions();
            foreach (var sub in subscriptions) {
                var missingChapters = await _subscriptionService.DownloadMissingChapters(sub);
                //todo notify about downloaded chapters

                if (missingChapters.Any()) {
                    missingChapters.ForEach(s => sub.KnownChapters.Add(s));
                    await _subscriptionRepository.Save(sub);
                }
            }
        }
    }
}
