using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;

namespace MangaScraper.Core.Scrapers.Manga {
  public delegate Task<IHtmlDocument> PageGetter(string url);

  public interface ISeriesParser {
    /// <summary>
    /// The name of the page or site this parser is able to parse
    /// </summary>
    string ProviderName { get; }

    Task<IEnumerable<(string name, string url)>> ListInstances(PageGetter pageGetter, IProgress<double> progress = null);

    IEnumerable<string> ChapterUrls(IHtmlDocument doc);
    string CoverUrl(IHtmlDocument doc);
    IChapterParser CreateChapter(string url);
  }


  public static class SeriesDownloaderExtensions {
    public static int NrOfChapters(this ISeriesParser d, IHtmlDocument doc) {
      return d.ChapterUrls(doc).Count();
    }

    public static IEnumerable<IChapterParser> Chapters(this ISeriesParser d, IHtmlDocument doc) {
      return d.ChapterUrls(doc).Select(d.CreateChapter).ToList();
    }
  }
}