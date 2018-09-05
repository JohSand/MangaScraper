using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Threading.Tasks;
using MangaScraper.Core.Helpers;
using MangaScraper.UI.Composition;
using MessagePack;
using SysPath = System.IO.Path;

namespace MangaScraper.Application.Persistence {
    public class MemFile : IMemCache {
        private static string DirectoryPath =>
            SysPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MangaScraper");

        private string Path { get; }

        public MemFile() : this(DirectoryPath) { }

        public MemFile(string directoryPath) {
            Path = SysPath.Combine(directoryPath, "cache.data");

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
        }


        public async Task WriteToDisk(IEnumerable<(string provider, string name, string url)> manga) =>
            await WriteToDisk(manga.Select(CreateMessage).ToArray());

        private static Message CreateMessage((string provider, string name, string url) m) =>
            new Message() {
                Name = m.name,
                Url = m.url,
                Provider = m.provider
            };

        private async Task WriteToDisk(Message[] msg) {
            var arr = MessagePackSerializer.Serialize(msg);
            using (var mmf = MemoryMappedFile.CreateFromFile(Path, FileMode.Create, null, arr.Length))
            using (var vs = mmf.CreateViewStream()) {
                await vs.WriteAsync(arr, 0, arr.Length);
            }
        }


        public async Task<(string provider, string name, string url)[]> GetAsync() => await ReadFromDiskAsync().Select(m => (m.Provider, m.Name, m.Url)).ToArrayAsync();

        private async Task<Message[]> ReadFromDiskAsync() {
            using (var mmf = MemoryMappedFile.CreateFromFile(Path, FileMode.Open))
            using (var vs = mmf.CreateViewStream()) {
                return await MessagePackSerializer.DeserializeAsync<Message[]>(vs);
            }
        }
    }
}