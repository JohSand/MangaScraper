using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MangaScraper.Core.Scrapers;
using MangaScraper.Core.Scrapers.Manga;

namespace MangaScraper.Application.Services {
  public interface IMangaIndex {
    Task<IEnumerable<MangaInfo>> FindMangas(Genre genres);
    Task<IEnumerable<MangaInfo>> FindMangas(string name);
    Task Update();
    void Stop();
    Task<string> GetCoverUrl(string provider, string url);
    Task<IEnumerable<IChapterParser>> Chapters(string provider, string url);

    Task DownloadChapter(IChapterParser parser, string basePath, IProgress<double> progress = null);
  }
}