using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using MangaScraper.Core.Scrapers;

namespace MangaScraper.UI.Presentation.Manga {
    public class GenresViewModel : PropertyChangedBase {
        /// <inheritdoc />
        public GenresViewModel() {
            SelectedItems.CollectionChanged += (sender, args) => NotifyOfPropertyChange(nameof(SelectedGenres));
        }

        private static T[] GetValues<T>() where T : Enum => (T[]) Enum.GetValues(typeof(T));

        public ObservableConcurrentDictionary<string, object> SelectedItems { get; set; } =
            new ObservableConcurrentDictionary<string, object>();

        public Dictionary<string, object> Items { get; } = GetValues<Genre>().ToDictionary(e => e.ToString(), e => (object) e);

        public Genre SelectedGenres => SelectedItems.Any() ? SelectedItems.Select(kvp => (Genre) kvp.Value).Merge() : Genre.None;
    }
}