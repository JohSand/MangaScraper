using MangaScraper.Application.Services;
using MangaScraper.Core.Helpers;
using MangaScraper.Core.Scrapers.Manga;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaScraper.Application.Subscriptions {

  public class SubscriptionItem {
    public string Provider { get; set; }
    public string Url { get; set; }
    public HashSet<string> KnownChapters { get; set; }

    public string Path {get;set;}
  }

  public class SubscriptionScheduler {
    private readonly SubscriptionService _subscriptionService;
    private readonly SubscriptionRepository _subscriptionRepository;

    public SubscriptionScheduler(SubscriptionService subscriptionService, SubscriptionRepository subscriptionRepository) {
      _subscriptionRepository = subscriptionRepository;
      _subscriptionService = subscriptionService;
    }

    public async Task Work() {
      var subscriptions = await _subscriptionRepository.GetSubscriptions();
      foreach(var sub in subscriptions) {
        var missingChapters = await _subscriptionService.DownloadMissingChapters(sub);

        foreach(var c in missingChapters)
          sub.KnownChapters.Add(c);

        _subscriptionRepository.Update(sub);
      }
      await _subscriptionRepository.Save();
    }
  }

  public class SubscriptionRepository {
    public Task<IEnumerable<SubscriptionItem>> GetSubscriptions() => throw new System.NotImplementedException("");

    public void Update(SubscriptionItem item) => throw new NotImplementedException("");

    public Task Save() => throw new NotImplementedException("");
  }

  public class SubscriptionService {
    public PageGetter PageGetter { get; }

    private IReadOnlyDictionary<string, ISeriesParser> Parsers { get; }

    private IFileSystem FileSystem { get; }


    public SubscriptionService(IFileSystem fileSystem, IEnumerable<ISeriesParser> parsers) {
      FileSystem = fileSystem;
      Parsers = parsers.ToDictionary(p => p.ProviderName, p => p);
      PageGetter = Client.GetDocumentAsync;
    }

    public async Task<IEnumerable<string>> DownloadMissingChapters(SubscriptionItem item) {
      var missingChapters = await ChapterParsers(item).ConfigureAwait(false);
      IProgress<double> ProgressForContext(string context) {
        //todo
        return null;
      }
      await missingChapters.Batch(5).Transform(s => DownloadChapterTo(s, item.Path, ProgressForContext(s.Number) ));
      return missingChapters.Select(s => s.Number);
    }

    public Task<IEnumerable<IChapterParser>> ChapterParsers(SubscriptionItem item) =>
      ChapterParsers(item.Provider, item.Url, item.KnownChapters);

    public async Task<IEnumerable<IChapterParser>> ChapterParsers(string provider, string url, HashSet<string> exclude) {
      var parser = Parsers[provider];
      return parser.ChapterUrls(await PageGetter(url)).Select(parser.CreateChapter).Where(c => !exclude.Contains(c.Number));
    }

    public Task DownloadChapterTo(IChapterParser parser, string basePath, IProgress<double> progress = null) {
      var path = Path.Combine(basePath, parser.Number);
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
      await WriteFileToPath(url, $"{path}\\{parser.Number}_{nr}.{extension ?? "jpg"}");
    }

    private async Task WriteFileToPath(string url, string fileName) {
      using (var fs = FileSystem.File.OpenWrite(fileName)) {
        await fs.DownloadToStream(url);
      }
    }
  }
}
