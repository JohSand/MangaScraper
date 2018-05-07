using System;
using System.Linq;
using Caliburn.Micro;
using MangaScraper.Application.Services;
using MangaScraper.Core.Helpers;
using MangaScraper.Core.Scrapers;
using MangaScraper.UI.Helpers;
using MangaScraper.UI.Main;

namespace MangaScraper.UI.Presentation.Manga {
  public class SearchViewModel : PropertyChangedBase {
    private IMangaIndex MangaIndex { get; }

    public SearchViewModel(IMangaIndex mangaIndex) {
      MangaIndex = mangaIndex;
      Instances = new BindableCollection<ProviderSetViewModel>();
      Genres = new GenresViewModel();
      Genres.OnPropertyChanges(t => t.SelectedGenres).Subscribe(a => {
        
      });
    }

    public async void FindMangas() {
      Instances.Clear();
      if (SearchString?.Length > 2) {
        var matches = (await MangaIndex.FindMangas(SearchString))          
          .Where(m => m.MetaData.Genres.HasFlag(Genres.SelectedGenres))
          .Select(g => new ProviderSetViewModel(g, MangaIndex))
          .Take(20);
        Instances.AddRange(matches);
      }
    }

    public async void UpdateButton_Click() {
      using (var pb = new ProgressWindow()) {
        pb.Show();
        await MangaIndex.Update();
      }
    }

    public async void SelectedRowsChanged() {
      //if no search string
      if ((SearchString?.Length ?? 0) <= 2) {
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

    public string SearchString { get; set; }
  }
}