using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using MangaScraper.Core.Helpers;

namespace MangaScraper.Core.Scrapers.Manga.Fox {
  public struct SeriesParser : ISeriesParser {
    public string ProviderName => "FanFox";

    public async Task<IEnumerable<(string name, string url)>> ListInstances(PageGetter pageGetter, IProgress<double> progress = null) {
      var mangaIndex = await pageGetter("http://fanfox.net/directory/").ConfigureAwait(false);
      var lastIndex = mangaIndex.GetElementById("nav").Element("ul").Elements("li").ToList();
      var secondLast = lastIndex[lastIndex.Count - 2];
      var index = int.Parse(secondLast.Element("a").TextContent.Trim());

      return await Enumerable.Range(1, index)
        .Select(i => $"http://fanfox.net/directory/{i}.htm")
        .Select(url => GetForUrl(pageGetter, url))
        .WhenAll(progress)
        .Flatten()
        .ToListAsync();
    }

    private static async Task<IEnumerable<(string, string)>> GetForUrl(PageGetter getter, string url) {
      var index = await getter(url).ConfigureAwait(false);
      return index
        .GetElementById("mangalist")
        .Element("ul")
        .Elements("li")
        .Select(n => {
          var element = n.GetElementsByClassName("manga_text").First().Element("a");
          return (element.TextContent, $"http:{element.GetAttribute("href")}");
        })
        .ToList();
    }

    public IEnumerable<string> ChapterUrls(IHtmlDocument doc) {
      return doc.GetElementById("chapters")? //div
               .Elements("ul")
               .SelectMany(ul => ul.Elements("li"))
               .Select(n => n.Element("div"))
               .Select(d => d.Element("h3") ?? d.Element("h4"))
               .Select(d => d.Element("a"))
               .Select(a => a.GetAttribute("href"))
               .Select(url => $"http:{url}")
               .ToList() ?? new List<string>();
    }

    public string CoverUrl(IHtmlDocument page) {
      return page
        .GetElementById("series_info")?//div
        .Elements("div")
        .First(e => e.HasClass("cover"))
        .Element("img")
        .Attributes.First(a => a.Name == "src").Value;
    }

    public IChapterParser CreateChapter(string url) => new ChapterParser(url);
  }
}