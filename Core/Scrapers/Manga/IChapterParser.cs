using System.Threading.Tasks;

namespace MangaScraper.Core.Scrapers.Manga {
  /// <summary>
  /// Implements the behaviour of finding the url of the image for a given page for a chapter
  /// </summary>
  public interface IChapterParser {
    
    Task<string> GetImageUrl(int pageNumber, PageGetter getPage);
    Task<int> GetPageCount(PageGetter getPage);
    string Url { get; }
    string Number { get; }
  }
}