using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Threading.Tasks;
using MangaScraper.Application.Persistence;
using MangaScraper.Core.Helpers;
using MessagePack;


namespace MangaScraper.UI.Composition {
    public class MemFile : IMemCache {
        private static string DirectoryPath => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                               System.IO.Path.DirectorySeparatorChar + "MangaScraper";

        private string Path { get; }

        public MemFile() {
            Path = DirectoryPath + System.IO.Path.DirectorySeparatorChar + "cache.data";

            if (!Directory.Exists(DirectoryPath))
                Directory.CreateDirectory(DirectoryPath);
        }


        public async Task WriteToDisk(IEnumerable<(string provider, string name, string url)> manga) {
            await WriteToDisk(manga.Select(m => new Message() {
                    Name = m.name,
                    Url = m.url,
                    Provider = m.provider
                })
                .ToArray());
        }

        private async Task WriteToDisk(Message[] msg) {
            var arr = MessagePackSerializer.Serialize(msg);
            using (var mmf = MemoryMappedFile.CreateFromFile(Path, FileMode.Create, null, arr.Length))
            using (var vs = mmf.CreateViewStream()) {
                await vs.WriteAsync(arr, 0, arr.Length);
            }
        }


        public async Task<(string provider, string name, string url)[]> GetAsync() =>
            await ReadFromDiskAsync().Select(m => (m.Provider, m.Name, m.Url)).ToArrayAsync();

        private async Task<Message[]> ReadFromDiskAsync() {
            using (var mmf = MemoryMappedFile.CreateFromFile(Path, FileMode.Open))
            using (var vs = mmf.CreateViewStream()) {
                return await MessagePackSerializer.DeserializeAsync<Message[]>(vs);
            }
        }
    }
}