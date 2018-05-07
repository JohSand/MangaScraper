//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.IO.MemoryMappedFiles;
//using System.Threading.Tasks;
//using MessagePack;
//using MessagePack.Resolvers;
//using Scraper.Model;
//using Scraper.Presentation.API;
//using Scraper.Presentation.Internal;
//using static System.Environment;
//using static System.IO.Path;

//namespace MangaScraper.UI.Composition {
//  public class MetaDataService : IMetaDataService {
//    private static string DirectoryPath => GetFolderPath(SpecialFolder.ApplicationData) + DirectorySeparatorChar + "MangaScraper";
//    private string Path { get; }
//    private IMetaDataProvider Provider { get; }

//    public MetaDataService(IMetaDataProvider provider) {
//      Path = DirectoryPath + DirectorySeparatorChar + "meta.data";

//      if (!Directory.Exists(DirectoryPath))
//        Directory.CreateDirectory(DirectoryPath);
//      Provider = provider;
//    }

//    private async Task WriteToDisk((string, MetaData)[] msg) {
//      var arr = MessagePackSerializer.Serialize(msg, ContractlessStandardResolver.Instance);
//      using (var mmf = MemoryMappedFile.CreateFromFile(Path, FileMode.Create, null, arr.Length))
//      using (var vs = mmf.CreateViewStream()) {
//        await vs.WriteAsync(arr, 0, arr.Length);
//      }
//    }

//    private async Task<(string Name, MetaData MetaData)[]> ReadFromDisk() {
//      using (var mmf = MemoryMappedFile.CreateFromFile(Path, FileMode.Open))
//      using (var vs = mmf.CreateViewStream()) {
//        return await MessagePackSerializer.DeserializeAsync<(string, MetaData)[]>(vs, ContractlessStandardResolver.Instance);
//      }
//    }

//    public async Task<IEnumerable<(string Name, MetaData MetaData)>> GetMetaData() {
//      var values = await ReadFromDisk();
//      return values;
//    }

//    public async Task UpdateAsync() {
//      var data = await Provider.GetMetaDataAsync().ToArrayAsync();
//      await WriteToDisk(data);
//    }
//  }
//}