using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using Caliburn.Micro;
using MangaScraper.Application.Services;
using MangaScraper.Core.Helpers;
using MangaScraper.Core.Scrapers;
using MangaScraper.UI.Helpers;
using MangaScraper.UI.Main;
using Reactive.Bindings;

namespace MangaScraper.UI.Presentation.Manga {
    public class SearchViewModel : PropertyChangedBase {
        private IMangaIndex MangaIndex { get; }

        public SearchViewModel(IMangaIndex mangaIndex) {
            MangaIndex = mangaIndex;
            Instances = new BindableCollection<ProviderSetViewModel>();
            Genres = new GenresViewModel();
            Genres.OnPropertyChanges(t => t.SelectedGenres).Select(a => SelectedRowsChanged()).Subscribe();
            SearchString = new ReactiveProperty<string>("");
            SearchString
                .Throttle(System.TimeSpan.FromMilliseconds(300))
                .ObserveOn(Dispatcher.CurrentDispatcher)
                .Select(s => { Instances.Clear(); return s; })
                .Where(s => s.Length > 3)

                .Select(FindMangas)
                
                .Subscribe();
        }

        public async Task FindMangas(string searchString) {           
            var mangas = await MangaIndex.FindMangas(searchString);
            var matches = mangas
                    .Where(m => m.MetaData.Genres.HasFlag(Genres.SelectedGenres))
                    .Take(20)
                    .Select(g => new ProviderSetViewModel(g, MangaIndex))
                    .ToList()
                ;
            Instances.AddRange(matches);
        }

        public async void UpdateButton_Click() {
            using (var pb = new ProgressWindow()) {
                pb.AddStacks(MangaIndex.Providers);
                pb.Show();
                await MangaIndex.Update(pb.GetProgress);
            }
        }

        public async Task SelectedRowsChanged() {
            //if no search string
            if ((SearchString.Value?.Length ?? 0) <= 2) {
                Instances.Clear();
                var asd = await MangaIndex.FindMangas(Genres.SelectedGenres).ToListAsync();
                Instances.AddRange(asd.Take(20).Select(g => new ProviderSetViewModel(g, MangaIndex)));
            }
            else {
                Genre selectedGenres = Genres.SelectedGenres;
                Instances.RemoveWhere(m => !m.MetaData.Genres.HasFlag(selectedGenres));
            }

            return;
        }

        public BindableCollection<ProviderSetViewModel> Instances { get; set; }

        public ProviderSetViewModel SelectedInstance { get; set; }

        public GenresViewModel Genres { get; set; }

        public ReactiveProperty<string> SearchString { get; }
    }
}