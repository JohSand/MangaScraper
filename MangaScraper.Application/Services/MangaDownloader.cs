using MangaScraper.Core.Helpers;
using MangaScraper.Core.Scrapers.Manga;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace MangaScraper.Application.Services {
    public class MangaDownloader : ParserServiceBase, IMangaDownloader {

        private IReadOnlyDictionary<string, ISeriesParser> Parsers { get; }

        public MangaDownloader(IFileSystem fileSystem, IEnumerable<ISeriesParser> parsers, PageGetter getter) : base(fileSystem, getter) =>
            Parsers = parsers.ToDictionary(p => p.ProviderName, p => p);

        public MangaDownloader(IFileSystem fileSystem, IEnumerable<ISeriesParser> parsers) : base(fileSystem) =>
            Parsers = parsers.ToDictionary(p => p.ProviderName, p => p);

        public Task<IEnumerable<(string name, string url)>> ListInstances(string provider, IProgress<double> progress = null) =>
            Parsers[provider].ListInstances(PageGetter, progress);

        public IEnumerable<string> Providers => Parsers.Keys;

        public async Task<string> CoverUrl(string provider, string url) =>
            Parsers[provider].CoverUrl(await PageGetter(url));

        public async Task<IEnumerable<IChapterParser>> ChapterParsers(string provider, string url) {
            var parser = Parsers[provider];
            return parser.ChapterUrls(await PageGetter(url)).Select(parser.CreateChapter);
        }
    }
}