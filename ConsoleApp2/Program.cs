using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MangaScraper.Application.Services;
using MangaScraper.Core.Scrapers.Manga.Kakalot;
using MangaScraper.UI.Composition;
using ShellProgressBar;


namespace MangaScraper.Core.Scrapers.Manga {
  class Program {
    static async Task<int> Main(string[] args) {
      //await Download();
      //await TestKakalot();

      //await Populate();
      await GetMetaData(new Eden.SeriesParser());
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

    private static async Task GetMetaData(IMetaDataParser parser) {
      var options = new ProgressBarOptions {
        ForegroundColor = ConsoleColor.Yellow,
        BackgroundColor = ConsoleColor.DarkYellow,
        ProgressCharacter = '─'
      };

      var cts = new CancellationTokenSource();
      var mgr = new MangaDownloader(null, new List<ISeriesParser>());
      var getter = mgr.PageGetter;
      var doc = await getter("https://www.mangaeden.com/en/en-manga/naruto/");
      //var doc = await getter("http://manganelo.com/manga/read_naruto_manga_online_free3");
      var metaData = parser.GetMetaData(doc);

      var service = new MetaDataService(parser);

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

      var t = service.Start(cts.Token, GetProgress);

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
      var getter = mgr.PageGetter;

      var chapter = fox.CreateChapter("http://mangakakalot.com/chapter/koushaku_reijou_no_tashinami/chapter_16");

      var nrPages = await chapter.GetPageCount(getter);
      var p1 = await chapter.GetImageUrl(6, getter);
      var p2 = await chapter.GetImageUrl(7, getter);
      var p3 = await chapter.GetImageUrl(19, getter);
      var mrg = new MangaDownloader(sys, new List<ISeriesParser> { fox });


      using (var pb = new ConsoleProgress(options)) {
        await mrg.DownloadChapterTo(chapter, @"C:\Pile\Test", pb);
      }
    }

    class ConsoleProgress : IProgress<double>, IDisposable {
      private readonly ProgressBar _bar;

      public ConsoleProgress(ProgressBarOptions options, string context = "test") => _bar = new ProgressBar(100, context, options);

      public void Report(double value) {
        _bar.Tick((int)(value * 100));
      }

      public void Dispose() {
        _bar?.Dispose();
      }
    }
  }
}