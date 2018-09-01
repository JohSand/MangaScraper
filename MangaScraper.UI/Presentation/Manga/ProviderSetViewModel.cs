using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using MangaScraper.Application.Services;
using MangaScraper.Core.Scrapers;
using MangaScraper.UI.Helpers;

namespace MangaScraper.UI.Presentation.Manga {
  public class ProviderSetViewModel : PropertyChangedBase {
    private readonly IMangaIndex _mangaIndex;

    public ProviderSetViewModel(MangaInfo mangaInfo, IMangaIndex mangaIndex) {
      _mangaIndex = mangaIndex;
      Name = mangaInfo.Name;
      Providers = mangaInfo.Instances.Select(t => new ProviderData{Provider = t.provider, Url = t.url}).ToBindableCollection();
      MetaData = mangaInfo.MetaData;
      this
        .OnPropertyChanges(s => s.SelectedProvider)
        .Subscribe(a => CreateChapterInstanceViewModel(a.Provider, a.Url));
    }

    private async Task CreateChapterInstanceViewModel(string provider, string url) {
      var coverUrl = await _mangaIndex.GetCoverUrl(provider, url);
      var chapters = await _mangaIndex.GetChapters(provider, url);
      SelectedInstance = new InstanceViewModel {
        Cover = string.IsNullOrEmpty(coverUrl) ? null : new BitmapImage(new Uri(coverUrl)),
        ChapterInstanceViewModel = new ChapterInstanceViewModel { Chapters = chapters },
        Name = Name,
        MetaData = MetaData
      };
    }

    public BindableCollection<ProviderData> Providers { get; set; }

    public MetaData MetaData { get; set; }
    public string Name { get; set; }

    public ProviderData SelectedProvider { get; set; }

    public InstanceViewModel SelectedInstance { get; set; }

    public void SetFirstProviderAsSelected() {
      if (SelectedProvider.Provider != null) {
        //if this has been set, then instance must also have been set, right?
        SelectedInstance.ChapterInstanceViewModel.SelectedRows.Clear();
        return;
      }
      SelectedProvider = Providers.First();
    }

    public struct ProviderData {
      public string Provider { get; set; }
      public string Url { get; set; }
    }
  }
}