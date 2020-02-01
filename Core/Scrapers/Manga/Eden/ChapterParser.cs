using System;
using System.Linq;
using System.Threading.Tasks;
using MangaScraper.Core.Helpers;

namespace MangaScraper.Core.Scrapers.Manga.Eden {
  public struct ChapterParser : IChapterParser {
    public ChapterParser(string url) {
      var thing = url.Split(new [] {'/'}, StringSplitOptions.RemoveEmptyEntries).ToList();
      Number = thing[thing.Count - 2];
      thing.RemoveAt(thing.Count -1);
      Url = "https://" + string.Join("/", thing.Skip(1)) + "/";
    }

    public async Task<string> GetImageUrl(int pageNumber, PageGetter getPage) {
      var page = await getPage(Url + "/" + pageNumber);
      return $"https:{page.GetElementById("mainImg").GetAttribute("src")}";
    }

    public async Task<int> GetPageCount(PageGetter getPage) {
      var page = await getPage(Url);
      return page?.GetElementById("pageSelect").Elements("option").Count() ?? 0;
    }

    public string Url { get; }
    public string Number { get; }
  }
}