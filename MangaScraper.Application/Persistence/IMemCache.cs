using System.Collections.Generic;
using System.Threading.Tasks;

namespace MangaScraper.Application.Persistence {
  public interface IMemCache {
    Task<(string provider, string name, string url)[]> GetAsync();

    Task WriteToDisk(IEnumerable<(string provider, string name, string url)> manga);
  }
}