using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using MangaScraper.Core.Helpers;
using MangaScraper.Core.Scrapers.Manga;

namespace MangaScraper.Application.Services {
  public class MangaDownloader : IMangaDownloader {
    public PageGetter PageGetter { get; }

    private IReadOnlyDictionary<string, ISeriesParser> Parsers { get; }

    private IFileSystem FileSystem { get; }


    public MangaDownloader(IFileSystem fileSystem, IEnumerable<ISeriesParser> parsers) {
      FileSystem = fileSystem;
      Parsers = parsers.ToDictionary(p => p.ProviderName, p => p);
      PageGetter = Client.GetCachedDocumentAsync;
    }

    public Task<IEnumerable<(string name, string url)>> ListInstances(string provider, IProgress<double> progress = null) {
      var parser = Parsers[provider];
      return parser.ListInstances(PageGetter, progress);
    }


    public IEnumerable<string> Providers => Parsers.Keys;

    public async Task<string> CoverUrl(string provider, string url) => 
      Parsers[provider].CoverUrl(await PageGetter(url));

    public async Task<IEnumerable<IChapterParser>> ChapterParsers(string provider, string url) {
      var parser = Parsers[provider];
      return parser.ChapterUrls(await PageGetter(url)).Select(parser.CreateChapter);
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