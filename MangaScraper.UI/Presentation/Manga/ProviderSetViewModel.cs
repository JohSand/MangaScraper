using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Caliburn.Micro;
using MangaScraper.Application.Services;
using MangaScraper.Core.Scrapers;
using MangaScraper.UI.Helpers;
using Reactive.Bindings;
using System.Reactive.Linq;

namespace MangaScraper.UI.Presentation.Manga {
    public class ProviderSetViewModel : PropertyChangedBase {
        private readonly IMangaIndex _mangaIndex;

        public ProviderSetViewModel(MangaInfo mangaInfo, IMangaIndex mangaIndex) {
            _mangaIndex = mangaIndex;
            Name = mangaInfo.Name;
            Providers = mangaInfo.Instances.Select(t => new ProviderData {Provider = t.provider, Url = t.url}).ToBindableCollection();
            MetaData = mangaInfo.MetaData;

            SelectedInstance = this
                .OnPropertyChanges(s => s.SelectedProvider)
                .SelectTask(a => CreateChapterInstanceViewModel(a.Provider, a.Url))
                //.ObserveOn(Dispatcher.CurrentDispatcher)
                .ToReactiveProperty();

            SelectedInstance
                .Subscribe(x => x?.ChapterInstanceViewModel.SelectedRows.Clear());
        }

        private async Task<InstanceViewModel> CreateChapterInstanceViewModel(string provider, string url) {
            var coverUrl = await _mangaIndex.GetCoverUrl(provider, url);
            var chapters = await _mangaIndex.GetChapters(provider, url);
            return new InstanceViewModel {
                Cover = string.IsNullOrEmpty(coverUrl) ? null : new BitmapImage(new Uri(coverUrl)),
                ChapterInstanceViewModel = new ChapterInstanceViewModel {Chapters = chapters},
                Name = Name,
                MetaData = MetaData
            };
        }

        public BindableCollection<ProviderData> Providers { get; set; }

        public MetaData MetaData { get; set; }
        public string Name { get; set; }

        public ProviderData SelectedProvider { get; set; }

        public ReactiveProperty<InstanceViewModel> SelectedInstance { get; set; }

        public struct ProviderData {
            public string Provider { get; set; }
            public string Url { get; set; }
        }
    }
}