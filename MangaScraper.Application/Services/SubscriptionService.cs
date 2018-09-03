using MangaScraper.Core.Scrapers.Manga;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaScraper.Application.Services {
  public class SubscriptionService {
    public PageGetter PageGetter { get; }

    private IReadOnlyDictionary<string, ISeriesParser> Parsers { get; }

    private IFileSystem FileSystem { get; }


    public SubscriptionService(IFileSystem fileSystem, IEnumerable<ISeriesParser> parsers) {
      FileSystem = fileSystem;
      Parsers = parsers.ToDictionary(p => p.ProviderName, p => p);
      PageGetter = Client.GetCachedDocumentAsync;
    }

    public async Task<IEnumerable<IChapterParser>> ChapterParsers(string provider, string url, HashSet<string> exclude) {
      var parser = Parsers[provider];
      return parser.ChapterUrls(await PageGetter(url)).Select(parser.CreateChapter).Where(c => !exclude.Contains(c.Number));
    }

    public Task DownloadChapterTo(IChapterParser parser, string basePath, IProgress<double> progress = null) {
      var path = basePath + "\\" + parser.Number + "\\";
      if (!FileSystem.Directory.Exists(path))
        FileSystem.Directory.CreateDirectory(path);
      return WritePages(parser, path, progress);
    }

    private async Task WritePages(IChapterParser parser, string path, IProgress<double> progress) {
      var nrOfPages = await parser.GetPageCount(PageGetter);
      await Enumerable.Range(1, nrOfPages).Select(nr => WritePage(parser, path, nr)).WhenAll(progress);
    }

    private async Task WritePage(IChapterParser parser, string path, int nr) {
      var url = await parser.GetImageUrl(nr, PageGetter);
      var extension = url.Split('.').LastOrDefault()?.Split('?').FirstOrDefault();
      //todo
      await WriteFileToPath(url, $"{path}{parser.Number}_{nr}.{extension ?? "jpg"}");
    }

    private async Task WriteFileToPath(string url, string fileName) {
      using (var fs = FileSystem.File.OpenWrite(fileName)) {
        await fs.DownloadToStream(url);
      }
    }
  }
}
