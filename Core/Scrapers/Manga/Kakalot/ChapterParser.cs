using System.Linq;
using System.Threading.Tasks;

namespace MangaScraper.Core.Scrapers.Manga.Kakalot {
  public struct ChapterParser : IChapterParser {

    public ChapterParser(string url) {
      Url = url;
      Number = url.Split('/').Last().Replace("chapter_", "");

    }

    public async Task<string> GetImageUrl(int pageNumber, PageGetter getPage) {
      var doc = await getPage(Url);
      return doc
        .GetElementById("vungdoc")
        .Children.Skip(pageNumber - 1)
        .First(c => c.LocalName == "img")
        .GetAttribute("src");
    }

    public async Task<int> GetPageCount(PageGetter getPage) {
      var doc = await getPage(Url);
      var vungdoc = doc.GetElementById("vungdoc");
      return vungdoc.Children.Count(c => c.LocalName == "img");
    }


    public string Url { get; }
    public string Number { get; }
  }
}