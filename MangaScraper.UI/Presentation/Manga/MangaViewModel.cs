using System;
using System.Linq;
using Caliburn.Micro;
using MangaScraper.UI.Composition;
using MangaScraper.UI.Helpers;
using Reactive.Bindings;

namespace MangaScraper.UI.Presentation.Manga {
    public class MangaViewModel : Screen, IPrimaryScreen {
        public MangaViewModel(SearchViewModel searchView) {
            SearchViewModel = searchView;
            ProviderSetViewModel = SearchViewModel
                .OnPropertyChanges(s => s.SelectedInstance)
                .ToReactiveProperty();
            ProviderSetViewModel
                .Subscribe(a => {
                    if (a != null && a.SelectedProvider.Provider == null) a.SelectedProvider = a.Providers.First();
                });
        }

        public int Order => 1;

        public override string DisplayName => "Search for manga";

        public SearchViewModel SearchViewModel { get; set; }

        public ReactiveProperty<ProviderSetViewModel> ProviderSetViewModel { get; set; }
    }
}