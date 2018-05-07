using System;
using Caliburn.Micro;
using MangaScraper.UI.Composition;
using MangaScraper.UI.Helpers;

namespace MangaScraper.UI.Presentation.Manga {
  public class MangaViewModel : Screen, IPrimaryScreen {
    public MangaViewModel(SearchViewModel searchView) {
      SearchViewModel = searchView;
      SearchViewModel
        .OnPropertyChanges(s => s.SelectedInstance)
        .Subscribe(a=> {
          ProviderSetViewModel = a;
          ProviderSetViewModel?.SetFirstProviderAsSelected();
        });
    }

    public int Order => 1;

    public override string DisplayName => "Search for manga";

    public SearchViewModel SearchViewModel { get; set; }

    public ProviderSetViewModel ProviderSetViewModel { get; set; }
  }
}