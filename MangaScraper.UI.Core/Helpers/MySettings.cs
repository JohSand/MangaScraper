using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Utf8Json;

namespace MangaScraper.UI.Core.Helpers {
  public class MySettings {
   // private readonly AsyncLock _lock = new AsyncLock();
    private readonly IDictionary<string, object> _expandoObject = new ExpandoObject();
    public string Name { get; } = $"MangaScraper{Path.DirectorySeparatorChar}settings.json";
    private string Base => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    private IFileSystem FileSystem { get; }

    public MySettings(IFileSystem fileSystem) => FileSystem = fileSystem;

    public object this[string s] {
      get => _expandoObject.ContainsKey(s) ? _expandoObject[s] : null;
      set {
      //  using (_lock.LockAsync().GetAwaiter().GetResult()) {
          _expandoObject[s] = value;
      //  }
      }
    }

    public void Save() => SaveAsync().GetAwaiter().GetResult();

    private async Task SaveAsync() {
    //  using (await _lock.LockAsync())
      using (var fs = FileSystem.File.Open($"{Base}{Path.DirectorySeparatorChar}{Name}", FileMode.Create)) {
        await JsonSerializer.SerializeAsync(fs, _expandoObject);
      }
    }
  }
}