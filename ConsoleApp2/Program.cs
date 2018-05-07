using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using MangaScraper.Application.Services;
using MangaScraper.Core.Helpers;
using MangaScraper.Core.Scrapers.Manga.Kakalot;
using MangaScraper.UI.Composition;
using ShellProgressBar;


namespace MangaScraper.Core.Scrapers.Manga {
  class Program {
    static async Task<int> Main(string[] args) {
      await Download();
      //await TestKakalot();

      //await Populate();
      //await GetMetaData();
      return 0;
    }

    private static async Task TestKakalot() {
      var options = new ProgressBarOptions {
        ForegroundColor = ConsoleColor.Yellow,
        BackgroundColor = ConsoleColor.DarkYellow,
        ProgressCharacter = '─'
      };
      var mrg = new MangaDownloader(new FileSystem(), new List<ISeriesParser> {  });


      for (int i = 4; i < 9; i++) {
        var url = $"http://mangakakalot.com/chapter/goblin_slayer_side_story_year_one/chapter_{i}";
        var chapterParser = new ChapterParser(url);

        using (var pb = new ConsoleProgress(options)) {
          await mrg.DownloadChapterTo(chapterParser, @"C:\Pile\Test", pb);
        }
      }
    }

    private static async Task Populate() {
      var options = new ProgressBarOptions {
        ForegroundColor = ConsoleColor.Yellow,
        BackgroundColor = ConsoleColor.DarkYellow,
        ProgressCharacter = '─'
      };

      var memCache = new MemFile();
      var panda = new Panda.SeriesParser();

      var manager = new MangaDownloader(new FileSystem(), new List<ISeriesParser> { panda });

      (IEnumerable<(string name, string url)> Result, string provider)[] res;
      using (var pb = new ConsoleProgress(options)) {
        var provider = manager.Providers.First();
        var thing = await manager.ListInstances(provider, pb);
        res = new (IEnumerable<(string name, string url)> Result, string provider)[] {
          (thing, provider)
        };
        //res = await Task.WhenAll(manager.Providers.Select(p => manager.ListInstances(p, pb).ContinueWith(t => (t.Result, provider: p))));
      }
      var asd = res.SelectMany(q => q.Result, (x, t) => (x.provider, t.name, t.url));
      await memCache.WriteToDisk(asd);
      var res2 = await memCache.GetAsync();
      //var res = await MyDictionary;
      //MyDictionary = new AsyncLazy<Dictionary<string, MangaInfo>>(CreateDictionary);
    }

    private static async Task GetMetaData() {
      var cts = new CancellationTokenSource();
      IMetaDataParser parser = new Eden.SeriesParser();
      var mgr = new MangaDownloader(null, new List<ISeriesParser>());
      var getter = mgr.PageGetter;
      var doc = await getter("https://www.mangaeden.com/en/en-manga/naruto/");
      var metaData = parser.GetMetaData(doc);

      var service = new MetaDataService(parser);

      var res1 = await service.GetMetaData();
      
      var t = service.Start(cts.Token);

      var res2 = await service.GetMetaData();

      cts.Cancel();
      try {
        await t;
      }
      catch (Exception e) {
        Console.WriteLine(e.Message);
      }
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

      var mrg = new MangaDownloader(sys, new List<ISeriesParser> { fox });

      List<(string name, string url)> mangas;
      using (var pb = new ConsoleProgress(options)) {
        mangas = await mrg.ListInstances(fox.ProviderName, pb).ToListAsync();
      }


      Console.WriteLine(mangas.Count);
      var (_, url) = mangas.Skip(8).First();
      var chapters = await mrg.ChapterParsers(fox.ProviderName, url).ToListAsync();
      var someChapter = chapters.First();
      using (var pb = new ConsoleProgress(options)) {
        await mrg.DownloadChapterTo(someChapter, @"C:\Pile\Test", pb);
      }
    }

    class ConsoleProgress : IProgress<double>, IDisposable {
      private readonly ProgressBar _bar;

      public ConsoleProgress(ProgressBarOptions options) => _bar = new ProgressBar(100, "test", options);

      public void Report(double value) {
        _bar.Tick((int)(value * 100));
      }

      public void Dispose() {
        _bar?.Dispose();
      }
    }
  }
}