using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MangaScraper.Application.Services;
using MangaScraper.Application.Subscriptions;
using MangaScraper.UI.Core.Helpers;
using Reactive.Bindings;

namespace MangaScraper.UI.Core.Presentation.Manga.SelectedManga.Chapters {
    public class SubscriptionViewModel : ChapterInstances {
        public delegate SubscriptionViewModel Factory((string, ProviderData) data, IEnumerable<ChapterInstance> chapters);

        public SubscriptionViewModel((string, ProviderData) data, IEnumerable<ChapterInstance> chapters, ISubscriptionRepository repository) : base(chapters) {
            //SelectedRows.CollectionChanged += (s, e) => NotifyOfPropertyChange(() => CanDownloadSelected);
            (Name, (Provider, Url)) = data;
            Repository = repository;
            Subscription = Observable
                .FromAsync(_ => Repository.GetSubscription(Name, Provider))
                .ToReactiveProperty();

        }

        private string Provider { get; }
        private string Name { get; }
        private string Url { get; }

        public async Task Create() {
            var (targetFolder, canceled) = FolderDialog.GetTargetFolder();
            if (canceled) return;

            var info = new SubscriptionItem {
                Name = Name,
                Provider = Provider,
                Path = targetFolder,
                Url = Url,
                KnownChapters = SelectedRows.Select(r => r.Number.TrimStart('0')).ToHashSet()
            };
            Subscription.Value = info;
            await Repository.Save(info);
        }

        private ISubscriptionRepository Repository { get; }

        public ReactiveProperty<SubscriptionItem> Subscription { get; }
    }
}