using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using MangaScraper.Core.Helpers;

namespace MangaScraper.Core.Scrapers.Manga.Kakalot {
  public struct SeriesParser : ISeriesParser {
    public string ProviderName => "MangaKakalot";

    public async Task<IEnumerable<(string name, string url)>> ListInstances(PageGetter pageGetter, IProgress<double> progress = null) {
      var doc = await pageGetter("http://mangakakalot.com/manga_list?type=topview&category=all&state=all&page=1");
      var groupPage = doc.GetElementsByClassName("group-page").First();
      var last = groupPage.Children.Last(c => c.LocalName == "a").TextContent.Replace("Last(", "").Replace(")", "");
      var index = int.Parse(last);
      return await Enumerable.Range(1, index)
        .Select(i => $"http://mangakakalot.com/manga_list?type=topview&category=all&state=all&page={i}")
        .Select(url => GetForUrl(pageGetter, url))
        .WhenAll(progress)
        .Flatten()
        .ToListAsync();
    }

    private static async Task<IEnumerable<(string, string)>> GetForUrl(PageGetter getter, string url) {
      var index = await getter(url).ConfigureAwait(false);
      return index
        .GetElementsByClassName("list-truyen-item-wrap")
        .Select(wrap => wrap.Element("a"))
        .Select(a => {
          var href = a.GetAttribute("href");
          var title = a.GetAttribute("title");
          return (title, href);
        })
        .ToList();
    }

    public IEnumerable<string> ChapterUrls(IHtmlDocument doc) {
      var chapterList = doc.GetElementsByClassName("chapter-list").First();
      return chapterList.Elements("div").Select(d => d.Element("span").Element("a").GetAttribute("href")).ToList();
    }

    public string CoverUrl(IHtmlDocument doc) {
      var pic = doc.GetElementsByClassName("manga-info-pic").First();
      var img = pic.Element("img");
      var src = img.GetAttribute("src");
      return src;
    }

    public IChapterParser CreateChapter(string url) => new ChapterParser(url);
  }
}