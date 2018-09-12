using MangaScraper.Application.Services;
using MangaScraper.Application.Subscriptions;
using MangaScraper.UI.Helpers;
using MangaScraper.UI.Presentation.Common;
using Reactive.Bindings;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace MangaScraper.UI.Presentation.Manga.SelectedManga.Chapters {
    public class SubscriptionViewModel : ChapterInstances {
        public delegate SubscriptionViewModel Factory((string, string) name, IEnumerable<ChapterInstance> chapters);

        public SubscriptionViewModel((string, string) name, IEnumerable<ChapterInstance> chapters, ISubscriptionRepository repository) : base(chapters) {
            SelectedRows.CollectionChanged += (s, e) => NotifyOfPropertyChange(() => CanDownloadSelected);
            Repository = repository;
            Subscription = Observable
                .FromAsync(_ => Repository.GetSubscription(("", "")))
                .ToReactiveProperty();
            (Name, Provider) = name;
        }

        private string Provider { get; }
        private string Name { get; }

        public async Task Create() {
            var info = new SubscriptionItem {
                Name = Name,
                Provider = Provider,
                Path = "todo",
                Url = "todo",

            };
            await this.Repository.Save(info);
        }

        public bool CanDownloadSelected => SelectedRows?.Any() == true;

        public void DownloadSelected() => Download(SelectedRows);

        public void DownloadManga_All() => Download(Chapters);

        public void DownloadManga_OnClick(ChapterInstance chapter) => Download(new[] { chapter });

        private void Download(IEnumerable<ChapterInstance> chaptersToDownload) {
            var (targetFolder, canceled) = FolderDialog.GetTargetFolder();
            if (canceled) return;

            
            var a = chaptersToDownload
                .Select(c => (c.Number, (AsyncWork)(p => c.DownloadTo(targetFolder, p))))
                .Reverse()
                .ToList();
            MultiProgress.ScheduleProgress(a);
        }

        public MultiProgressViewModel MultiProgress { get; set; } =
            new MultiProgressViewModel {
                ErrorLog = (s, exception) => { }
            };

        private ISubscriptionRepository Repository { get; }

        public ReactiveProperty<SubscriptionItem> Subscription { get; }
    }
}