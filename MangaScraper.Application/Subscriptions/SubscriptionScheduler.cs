using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MangaScraper.Application.Subscriptions {
    public class SubscriptionScheduler : ISubscriptionScheduler {
        private readonly ISubscriptionService _subscriptionService;
        private readonly ISubscriptionRepository _subscriptionRepository;

        public SubscriptionScheduler(ISubscriptionService subscriptionService, ISubscriptionRepository subscriptionRepository) {
            _subscriptionRepository = subscriptionRepository;
            _subscriptionService = subscriptionService;
        }

        public event EventHandler<NewChaptersEventArgs> NewChapters;

        public Task Start(CancellationToken token) {
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
            var x = subscriptions.ToArray();
            foreach (var sub in x) {
                var missingChapters = await _subscriptionService.DownloadMissingChapters(sub);
                //todo notify about downloaded chapters
                var arg = new NewChaptersEventArgs(sub.Name, missingChapters);
                NewChapters?.Invoke(this, arg);
                if (missingChapters.Any()) {
                    missingChapters.ForEach(s => sub.KnownChapters.Add(s));
                    await _subscriptionRepository.Save(sub);
                }
            }
        }
    }
}
