using MangaScraper.Core.Helpers;
using MangaScraper.Core.Scrapers;
using MangaScraper.Core.Scrapers.Manga;
using MessagePack.Resolvers;
using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static MessagePack.MessagePackSerializer;
using static System.Environment;
using static System.IO.Path;

namespace MangaScraper.Application.Services {
    public class MetaDataService : IMetaDataService {
        private static string DirectoryPath => Combine(GetFolderPath(SpecialFolder.ApplicationData), "MangaScraper");

        public MetaDataService(IMetaDataParser metaDataParser) {
            MetaDataParser = metaDataParser;
            PageGetter = Client.GetDocumentAsync;

            Path = Combine(DirectoryPath, "meta.data");

            if (!Directory.Exists(DirectoryPath))
                Directory.CreateDirectory(DirectoryPath);

            if (!File.Exists(Path)) {
                WriteToDisk(new (string, MetaData)[0]).GetAwaiter().GetResult();
            }
        }

        private readonly AsyncLock _lock = new AsyncLock();
        private readonly object _progressLock = new object();
        private GetProgress _reportProgressFactory;

        private IMetaDataParser MetaDataParser { get; }

        private PageGetter PageGetter { get; }

        private string Path { get; }

        public GetProgress ReportProgressFactory {
            get {
                lock (_progressLock) {
                    return _reportProgressFactory;
                }
            }
            set {
                lock (_progressLock)
                    _reportProgressFactory = value;
            }
        }


        public async Task<(string name, MetaData metaData)[]> GetMetaData() {
            using (await _lock.LockAsync()) {
                return await ReadFromDisk().ConfigureAwait(false);
            }
        }

        public Task Start(CancellationToken token) {
            var scheduler = new SingleThreadTaskScheduler(ApartmentState.MTA);

            async Task EventLoop() {
                using (scheduler) {
                    while (!token.IsCancellationRequested) {
                        var thing = await DownloadMetaData(token);
                        token.ThrowIfCancellationRequested();
                        using (await _lock.LockAsync()) {
                            await WriteToDisk(thing).ConfigureAwait(false);
                        }
                        await Task.Delay(600 * 1000, token);
                    }
                }
            }

            return Task.Factory.StartNew(
                EventLoop,
                token,
                TaskCreationOptions.None,
                scheduler
              )
              .Unwrap();
        }

        private async Task<(string, MetaData)[]> DownloadMetaData(CancellationToken token) {
            var progress = ReportProgressFactory?.Invoke("Instances");
            var instances = await MetaDataParser.ListInstances(PageGetter, progress).ToListAsync();
            var p = ReportProgressFactory?.Invoke("MetaData");
            return await instances
                .Batch(20)
                .Transform(t => GetMetaData(t), token, p, 1000)
                .ToArrayAsync();
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
            var arr = Serialize(msg, ContractlessStandardResolver.Instance);
            using (var mmf = MemoryMappedFile.CreateFromFile(Path, FileMode.Create, null, arr.Length))
            using (var vs = mmf.CreateViewStream()) {
                await vs.WriteAsync(arr, 0, arr.Length).ConfigureAwait(false);
            }
        }

        private async Task<(string, MetaData)[]> ReadFromDisk() {
            using (var mmf = MemoryMappedFile.CreateFromFile(Path, FileMode.Open))
            using (var vs = mmf.CreateViewStream()) {
                return await DeserializeAsync<(string, MetaData)[]>(vs, ContractlessStandardResolver.Instance).ConfigureAwait(false);
            }
        }
    }
}