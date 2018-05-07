using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MangaScraper.Core.Scrapers.Manga {
  public interface IMetaDataService {
    //todo
    Task<(string name, MetaData metaData)[]> GetMetaData();

    Task Start(CancellationToken token);
  }
}