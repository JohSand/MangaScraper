using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using MangaScraper.Core.Helpers;

namespace MangaScraper.Core.Scrapers.Manga.Fun {
  public struct SeriesParser : ISeriesParser {
    public string ProviderName => "FunManga";

    public async Task<IEnumerable<(string name, string url)>> ListInstances(PageGetter pageGetter, IProgress<double> progress = null) {
      return await GetAlphabet().Select(s => GetForUrl(pageGetter, s)).WhenAll(progress).Flatten().ConfigureAwait(false);
    }

    private static IEnumerable<string> GetAlphabet() {
      yield return "";

      for (int i = 0; i < 10; i++) {
        yield return i.ToString();
      }
      for (var c = 'A'; c <= 'Z'; c++) {
        yield return c.ToString();
      }
    }

    private static async Task<IEnumerable<(string, string)>> GetForUrl(PageGetter getter, string letter) {
      var mangaIndex = await getter($"http://funmanga.com/manga-list/{letter}").ConfigureAwait(false);
      return mangaIndex
        .GetElementsByTagName("ul")
        .Where(n => n.HasClass("manga-list"))
        .SelectMany(listNode => listNode.Elements("li"))
        .Select(n => n.Element("a"))
        .Select(e => (e.TextContent, e.Attributes.FirstOrDefault(a => a.Name == "href")?.Value));
    }

    public IEnumerable<string> ChapterUrls(IHtmlDocument doc) {
      return doc
        .GetElementsByTagName("ul")
        .First(n => n.HasClass("chapter-list"))
        .Elements("li")
        .Select(n => n.Children.First(a => a.LocalName == "a"))
        .Select(n => n.Attributes.First(a => a.Name == "href").Value)
        .OrderBy(s => s)
        .ToList();
    }

    public string CoverUrl(IHtmlDocument page) {
      var img = page.GetElementsByTagName("img").First(e => e.ClassList.Contains("img-responsive"));
      return img.Attributes.Single(a => a.Name == "src").Value;
    }

    public IChapterParser CreateChapter(string url) => new ChapterParser(url);
  }
}