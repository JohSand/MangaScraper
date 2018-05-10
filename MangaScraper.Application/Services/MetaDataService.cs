using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using MangaScraper.Application.Poc.Sqltabledependency;
using MangaScraper.Core.Scrapers;
using MangaScraper.Core.Scrapers.Manga;
using MangaScraper.UI.Composition;
using MessagePack;

namespace MangaScraper.Application.Services {
  public class MetaDataService : IMetaDataService {
    private static string DirectoryPath => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                           System.IO.Path.DirectorySeparatorChar + "MangaScraper";

    public MetaDataService(IMetaDataParser metaDataParser) {
      MetaDataParser = metaDataParser;
      PageGetter = Client.GetDocumentAsync;

      Path = DirectoryPath + System.IO.Path.DirectorySeparatorChar + "meta.data";

      if (!Directory.Exists(DirectoryPath))
        Directory.CreateDirectory(DirectoryPath);
    }

    private readonly AsyncLock _lock = new AsyncLock();

    private IMetaDataParser MetaDataParser { get; }

    public PageGetter PageGetter { get; }

    public string Path { get; }

    public async Task<(string name, MetaData metaData)[]> GetMetaData() {
      using (_lock.LockAsync()) {
        return await ReadFromDisk();
      }
    }


    //todo, this is way too agressive, and steals all network resources. Throttle
    public Task Start(CancellationToken token) => Start(token, null);

    public Task Start(CancellationToken token, IProgress<double> progress) =>
      Task.Factory.StartNew(
        () => EventLoop(token, progress),
        token,
        TaskCreationOptions.None,
        new SingleThreadTaskScheduler(ApartmentState.MTA)
      ).Unwrap();

    private async Task EventLoop(CancellationToken token, IProgress<double> progres = null) {
      token.ThrowIfCancellationRequested();
      var thing = await DownloadMetaData(progres);
      using (_lock.LockAsync()) {
        await WriteToDisk(thing);
      }
      await Task.Delay(10 * 1000, token);
    }

    private async Task<(string, MetaData)[]> DownloadMetaData(IProgress<double> progress = null) {
      var instances = await MetaDataParser.ListInstances(PageGetter, progress);
      var throttle = instances.Select((a, i) => (item: a, index: i))
        .GroupBy(t => t.index / 20)
        .Select(g => g.Select(x => x.item))
        .ToList();

      List< (string, MetaData) > thing = new List<(string, MetaData)>();
      foreach (var list in throttle) {
        thing.AddRange(await Task.WhenAll(list.Select(GetMetaData)));
      }
      return thing.ToArray();
      //return await Task.WhenAll(instances.Select(GetMetaData));
    }

    public async Task<(string, MetaData)> GetMetaData((string name, string url) valueTuple) {
      try {
        var doc = await PageGetter(valueTuple.url);
        return (valueTuple.name, MetaDataParser.GetMetaData(doc));
      }
      catch (Exception e) {
        Console.WriteLine(e);
        return (null, new MetaData());
      }
    }


    private async Task WriteToDisk((string, MetaData)[] msg) {
      var arr = MessagePackSerializer.Serialize(msg, MessagePack.Resolvers.ContractlessStandardResolver.Instance);
      using (var mmf = MemoryMappedFile.CreateFromFile(Path, FileMode.Create, null, arr.Length))
      using (var vs = mmf.CreateViewStream()) {
        await vs.WriteAsync(arr, 0, arr.Length);
      }
    }

    private async Task<(string, MetaData)[]> ReadFromDisk() {
      using (var mmf = MemoryMappedFile.CreateFromFile(Path, FileMode.Open))
      using (var vs = mmf.CreateViewStream()) {
        return await MessagePackSerializer.DeserializeAsync<(string, MetaData)[]>(vs, MessagePack.Resolvers.ContractlessStandardResolver.Instance);
      }
    }
  }
}