using System.Linq;
using System.Threading.Tasks;
using MangaScraper.Core.Helpers;

namespace MangaScraper.Core.Scrapers.Manga.Fox {
  public struct ChapterParser : IChapterParser {
    public ChapterParser(string url) {

      Url = url;
      var thing = url.Split('/').ToList();
      Number = thing[thing.Count - 2].Replace("c", "");
    }


    public async Task<string> GetImageUrl(int pageNumber, PageGetter getPage) {
      var strippedUrl = Url.Split('/');
      var url = string.Join("/", strippedUrl.Take(strippedUrl.Length - 1));
      var page = await getPage(url + "/" + pageNumber + ".html");
      var imageUrl = page
        .GetElementById("viewer")
        .Element("div")//class read_img
        .Element("a")
        .Element("img")
        .Attributes.First(a => a.Name == "src").Value;
      return imageUrl;
    }

    public async Task<int> GetPageCount(PageGetter getPage) {
      var doc = await getPage(Url);
      return doc
               .GetElementById("top_bar")
               .Element("div")
               .Element("div")
               .Element("select")
               .Elements("option")
               .Count() - 1;
    }

    public string Url { get; }
    public string Number { get; }
  }
}