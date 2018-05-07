using System;
using System.Threading.Tasks;
using MangaScraper.Core.Scrapers.Manga;

namespace MangaScraper.Application.Services {
  public class ChapterInstance {
    private string _number;

    public string Number {
      get => _number?.Length > MaxDigits ? _number : _number?.PadLeft(MaxDigits, '0');
      set => _number = value;
    }


    public int MaxDigits { get; set; }
    public IChapterParser Parser { get; set; }
    public IMangaIndex Index { get; set; }

    public Task DownloadTo(string targetFolder, IProgress<double> progress) => 
      Index.DownloadChapter(Parser, targetFolder, progress);
  }
}