using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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
                        try {
                            await Work();
                        }
                        catch {
                            //
                        }
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
            var subscriptions = await _subscriptionRepository.GetSubscriptions().ConfigureAwait(false);
            var missingChapterEvents = await Task.WhenAll(subscriptions.Select(DownloadMissing)).ConfigureAwait(false);
            foreach (var chapterEvent in missingChapterEvents) {
             if(chapterEvent.Chapters.Any())   
                 NewChapters?.Invoke(this, chapterEvent);
            }
            
        }

        private async Task<NewChaptersEventArgs> DownloadMissing(SubscriptionItem sub) {
            var missingChapters = await _subscriptionService.DownloadMissingChapters(sub).ConfigureAwait(false);

            if (!missingChapters.Any()) 
                return new NewChaptersEventArgs(sub.Name, missingChapters);

            sub.KnownChapters.UnionWith(missingChapters);
            await _subscriptionRepository.Save(sub).ConfigureAwait(false);
            return new NewChaptersEventArgs(sub.Name, missingChapters);
        }
    }
}
