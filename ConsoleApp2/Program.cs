using MangaScraper.Application.Services;
using MangaScraper.UI.Composition;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MangaScraper.Application.Persistence;


namespace MangaScraper.Core.Scrapers.Manga {
    internal class Program {
        private static async Task<int> Main(string[] args) {
            //await Download();
            //await TestKakalot();

            await Populate(
                //new Panda.SeriesParser(),
                //new Kakalot.SeriesParser(),
                //new Fun.SeriesParser()                , 
                new Eden.SeriesParser()
            );
            //await GetMetaData(new Eden.SeriesParser());
            return 0;
        }


        private static async Task TestKakalot() {
            var options = new ProgressBarOptions {
                ForegroundColor = ConsoleColor.Yellow,
                BackgroundColor = ConsoleColor.DarkYellow,
                ProgressCharacter = '─'
            };
            var mrg = new MangaDownloader(new FileSystem(), new List<ISeriesParser> { });


            for (int i = 4; i < 9; i++) {
                var url = $"http://mangakakalot.com/chapter/goblin_slayer_side_story_year_one/chapter_{i}";
                var chapterParser = new Kakalot.ChapterParser(url);

                using (var pb = new ConsoleProgress(options)) {
                    await mrg.DownloadChapterTo(chapterParser, @"C:\Pile\Test", pb);
                }
            }
        }

        private static async Task Populate(params ISeriesParser[] parser) {
            var options = new ProgressBarOptions {
                ForegroundColor = ConsoleColor.Yellow,
                BackgroundColor = ConsoleColor.DarkYellow,
                ProgressCharacter = '─'
            };

            var memCache = new MemFile();

            var manager = new MangaDownloader(new FileSystem(), parser);
            var index = new MangaIndex(manager, null, memCache);

            var first = true;
            ConsoleProgress pb = null;

            IProgress<double> GetProgress(string context) {
                pb?.Dispose();
                if (first)
                    first = false;
                else {
                    Console.WriteLine();
                    Console.WriteLine();
                }

                return pb = new ConsoleProgress(options, context);
            }

            try {
                await index.Update(GetProgress);
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
            //res = await Task.WhenAll(manager.Providers.Select(p => manager.ListInstances(p, pb).ContinueWith(t => (t.Result, provider: p))));


            //var res = await MyDictionary;
            //MyDictionary = new AsyncLazy<Dictionary<string, MangaInfo>>(CreateDictionary);
        }

        private static async Task GetMetaData(IMetaDataParser parser) {
            var options = new ProgressBarOptions {
                ForegroundColor = ConsoleColor.Yellow,
                BackgroundColor = ConsoleColor.DarkYellow,
                ProgressCharacter = '─'
            };

            var cts = new CancellationTokenSource();
            var mgr = new MangaDownloader(null, new List<ISeriesParser>());
            PageGetter getter = Client.GetDocumentAsync;
            var doc = await getter("https://www.mangaeden.com/en/en-manga/naruto/");
            //var doc = await getter("http://manganelo.com/manga/read_naruto_manga_online_free3");
            var metaData = parser.GetMetaData(doc);

            var service = new MetaDataService(new List<IMetaDataParser>() {parser});

            var wasCalled = false;

            IProgress<double> GetProgress(string context) {
                if (context == "Instances" && !wasCalled) {
                    wasCalled = true;
                    Console.WriteLine($"Handling {context}");
                    Console.WriteLine();
                    return new ConsoleProgress(options, context);
                }

                if (context == "MetaData") {
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine($"Handling {context}");
                    Console.WriteLine();
                    return new ConsoleProgress(options, context);
                }

                cts.Cancel();
                return null;
            }

            service.ReportProgressFactory = GetProgress;
            var t = service.Start(parser.ProviderName, cts.Token);

            try {
                await t;
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }

            var res2 = await service.GetMetaData();
            var test = res2.Where(a => a.metaData.Genres.HasFlag(Genre.MartialArts)).ToList();
            var unused = Enum.GetValues(typeof(Genre)).Cast<Genre>().Where(e => e != Genre.None).ToDictionary(g => g, _ => false);
            foreach (var valueTuple in res2) {
                foreach (var genre in valueTuple.metaData.Genres.Split()) {
                    if (unused.ContainsKey(genre))
                        unused[genre] = true;
                }
            }

            var areNotUsed = unused.Where(kvp => !kvp.Value).Select(kvp => kvp.Key).ToList();
            Console.Read();
        }

        private static async Task Download() {
            var options = new ProgressBarOptions {
                ForegroundColor = ConsoleColor.Yellow,
                BackgroundColor = ConsoleColor.DarkYellow,
                ProgressCharacter = '─'
            };

            Console.WriteLine("Hello World!");
            Console.WriteLine("");
            IFileSystem sys = new FileSystem();
            var fox = new Kakalot.SeriesParser();
            var mgr = new MangaDownloader(null, new List<ISeriesParser>());
            PageGetter getter = Client.GetDocumentAsync;

            var chapter = fox.CreateChapter("http://mangakakalot.com/chapter/koushaku_reijou_no_tashinami/chapter_16");

            var nrPages = await chapter.GetPageCount(getter);
            var p1 = await chapter.GetImageUrl(6, getter);
            var p2 = await chapter.GetImageUrl(7, getter);
            var p3 = await chapter.GetImageUrl(19, getter);
            var mrg = new MangaDownloader(sys, new List<ISeriesParser> {fox});


            using (var pb = new ConsoleProgress(options)) {
                await mrg.DownloadChapterTo(chapter, @"C:\Pile\Test", pb);
            }
        }

        private class ConsoleProgress : IProgress<double>, IDisposable {
            private readonly ProgressBar _bar;

            public ConsoleProgress(ProgressBarOptions options, string context = "test") => _bar = new ProgressBar(100, context, options);

            public void Report(double value) => _bar.Tick((int) (value * 100));

            public IProgress<double> GetProgress() => new Progress<double>(Report);

            public void Dispose() => _bar?.Dispose();
        }
    }
}