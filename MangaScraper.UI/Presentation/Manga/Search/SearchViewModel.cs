using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using MangaScraper.Application.Services;
using MangaScraper.Core.Scrapers;
using MangaScraper.UI.Helpers;
using MangaScraper.UI.Main;

namespace MangaScraper.UI.Presentation.Manga.Search {
    public class SearchViewModel : PropertyChangedBase {
        private IMangaIndex MangaIndex { get; }
        public ProviderSetViewModel.Factory Factory { get; }

        public SearchViewModel(IMangaIndex mangaIndex, ProviderSetViewModel.Factory factory) {
            MangaIndex = mangaIndex;
            Factory = factory;
            Genres = new GenresViewModel();
            Instances = this.OnPropertyChanges(s => s.SearchString)
                .Throttle(TimeSpan.FromMilliseconds(500), DispatcherScheduler.Current)
                .SelectTask(FindMangas)
                .Merge(Genres.OnPropertyChanges(t => t.SelectedGenres).SelectTask(SelectedGenreChanged))
                .Merge(this.OnPropertyChanges(t => t.ArtistSearchString)
                           .Throttle(TimeSpan.FromMilliseconds(500), DispatcherScheduler.Current)
                           .SelectTask(FindMangasByArtist))
                .ToReactiveCollection();
        }

        public async Task<List<ProviderSetViewModel>> FindMangas(string searchString) {
            if (searchString.Length <= 3)
                return new List<ProviderSetViewModel>();
            var mangas = await MangaIndex.FindMangas(searchString);
            return WrapMangas(mangas);
        }

        public async Task<List<ProviderSetViewModel>> FindMangasByArtist(string searchString) {
            if (searchString.Length <= 4)
                return new List<ProviderSetViewModel>();
            var mangas = await MangaIndex.FindMangasByArtist(searchString);
            return WrapMangas(mangas);
        }

        private List<ProviderSetViewModel> WrapMangas(IEnumerable<MangaInfo> mangas) => 
            mangas
                .Where(m => m.MetaData.Genres.HasFlag(Genres.SelectedGenres))
                .Take(20)
                .Select(g => Factory(g))
                .ToList();

        public async Task<List<ProviderSetViewModel>> SelectedGenreChanged(Genre genre) {
            if ((SearchString?.Length ?? 0) > 3)
                return Instances.Where(m => m.MetaData.Genres.HasFlag(genre)).ToList();
            //if no search string .Where(kvp => kvp.Name.ToLowerInvariant().Contains(lower))
            var mangas = await MangaIndex.FindMangas(genre).ConfigureAwait(false);
            return mangas.Take(20).Select(g => Factory(g)).ToList();
        }

        public async void UpdateButton_Click() {
            using (var pb = new ProgressWindow()) {
                pb.AddStacks(MangaIndex.Providers);
                pb.Show();
                await MangaIndex.Update(pb.GetProgress);
            }
        }

        public IObservableCollection<ProviderSetViewModel> Instances { get; set; }

        public ProviderSetViewModel SelectedInstance { get; set; }

        public GenresViewModel Genres { get; set; }

        public string SearchString { get; set; }

        public string ArtistSearchString { get; set; }
    }
}