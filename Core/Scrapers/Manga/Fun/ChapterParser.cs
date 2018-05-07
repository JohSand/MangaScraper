using System.Linq;
using System.Threading.Tasks;

namespace MangaScraper.Core.Scrapers.Manga.Fun {
  public struct ChapterParser : IChapterParser {

    public ChapterParser(string url) {
      Url = url;
      Number = url.Split('/').Last();
    }


    public async Task<string> GetImageUrl(int pageNumber, PageGetter getPage) {
      var page = await getPage(Url + "/" + pageNumber);
      return page.GetElementById("chapter_img").Attributes.First(a => a.Name == "src").Value;
    }

    public async Task<int> GetPageCount(PageGetter getPage) {
      var doc = await getPage(Url);
      var selectNode = doc.GetElementsByTagName("select").First(); //this is a bit iffy
      return selectNode.Children.Count(n => n.LocalName == "option") - 1;
    }

    public string Url { get; }
    public string Number { get; }
  }
}