using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MangaScraper.Core.Scrapers.Manga {
  public interface IMangaDownloader {
    IEnumerable<string> Providers { get; }
    Task<string> CoverUrl(string provider, string url);

    Task<IEnumerable<IChapterParser>> ChapterParsers(string provider, string url);
    Task DownloadChapterTo(IChapterParser parser, string basePath, IProgress<double> progress = null);

    Task<IEnumerable<(string name, string url)>> ListInstances(string provider, IProgress<double> progress = null);


  }
}