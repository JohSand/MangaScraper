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
      Items = ((Genre[]) Enum.GetValues(typeof(Genre))).ToDictionary(e => e.ToString(), e => (object) e);
      SelectedItems.CollectionChanged += (sender, args) => NotifyOfPropertyChange(nameof(SelectedGenres));
    }

    public ObservableConcurrentDictionary<string, object> SelectedItems { get; set; } = new ObservableConcurrentDictionary<string, object>();

    public Dictionary<string, object> Items { get; set; }

    public Genre SelectedGenres => SelectedItems.Any() ? SelectedItems.Select(kvp => (Genre) kvp.Value).Merge() : Genre.None;


  }
}