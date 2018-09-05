using MangaScraper.Core.Helpers;
using MangaScraper.Core.Scrapers.Manga;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace MangaScraper.Application.Subscriptions {
    public class SubscriptionService : ParserServiceBase {
        private IReadOnlyDictionary<string, ISeriesParser> Parsers { get; }

        public SubscriptionService(IFileSystem fileSystem, IEnumerable<ISeriesParser> parsers) : base(fileSystem) => 
            Parsers = parsers.ToDictionary(p => p.ProviderName, p => p);

        public async Task<IEnumerable<string>> DownloadMissingChapters(SubscriptionItem item) {
            var missingChapters = await ChapterParsers(item).ConfigureAwait(false);
            IProgress<double> ProgressForContext(string context) {
                //todo
                return null;
            }
            await missingChapters.Batch(5).Transform(s => DownloadChapterTo(s, item.Path, ProgressForContext(s.Number)));
            return missingChapters.Select(s => s.Number);
        }

        public Task<IEnumerable<IChapterParser>> ChapterParsers(SubscriptionItem item) =>
          ChapterParsers(item.Provider, item.Url, item.KnownChapters);

        public async Task<IEnumerable<IChapterParser>> ChapterParsers(string provider, string url, HashSet<string> exclude) {
            var parser = Parsers[provider];
            return parser.ChapterUrls(await PageGetter(url)).Select(parser.CreateChapter).Where(c => !exclude.Contains(c.Number));
        }
    }
}
