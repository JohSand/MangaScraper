using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using MangaScraper.Core.Helpers;

namespace MangaScraper.Core.Scrapers.Manga.Panda {
  public struct SeriesParser : ISeriesParser {
    public string ProviderName => "MangaPanda";

    public async Task<IEnumerable<(string name, string url)>> ListInstances(PageGetter pageGetter, IProgress<double> progress = null) {
      var mangaIndex = await pageGetter("http://www.mangapanda.com/alphabetical").ConfigureAwait(false);

      return mangaIndex
        .GetElementById("wrapper_body")
        .Element("div") //content_bloc2
        .Elements("div")
        .Where(n => n.HasClass("series_col")) //series_col
        .SelectMany(n => n.Elements("div") /* series_alpha */)
        .SelectMany(n => n.Elements("ul"))
        .SelectMany(ul => ul.Elements("li"))
        .Select(li => li.Element("a"))
        .Select(a => (a.TextContent, $"http://www.mangapanda.com{a.GetAttribute("href")}"))
        .Distinct()
        .ToList();
    }

    public IEnumerable<string> ChapterUrls(IHtmlDocument doc) {
      return doc
        .GetElementById("listing")
        .Elements("tr")
        .Where(n => !n.HasClass("table_head"))
        .Select(n => n.Element("td"))
        .Select(d => d.Element("a"))
        .Select(a => a.GetAttribute("href"))
        .Select(url => $"http://mangapanda.com{url}")
        .ToList();
    }

    public string CoverUrl(IHtmlDocument page) =>
      page.GetElementById("mangaimg").Element("img").GetAttribute("src");

    public IChapterParser CreateChapter(string url) => new ChapterParser(url);
  }
}