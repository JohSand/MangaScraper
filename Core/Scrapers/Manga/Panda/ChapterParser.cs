using System.Linq;
using System.Threading.Tasks;

namespace MangaScraper.Core.Scrapers.Manga.Panda {
  public struct ChapterParser : IChapterParser {
    public ChapterParser(string url) {
      Url = url;
      Number = Number = url.Split('/').Last();
    }


    public async Task<string> GetImageUrl(int pageNumber, PageGetter getPage) {
      var page = await getPage(Url + "/" + pageNumber);
      var imageUrl = page.GetElementById("img").Attributes.First(a => a.Name == "src").Value;
      return imageUrl;
    }

    public async Task<int> GetPageCount(PageGetter getPage) {
      var doc = await getPage(Url);
      return doc
        .GetElementById("pageMenu")
        .Children
        .Count(n => n.LocalName == "option");
    }

    public string Url { get; }
    public string Number { get; }
  }
}