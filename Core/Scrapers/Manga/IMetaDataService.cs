using System;
using System.Threading;
using System.Threading.Tasks;

namespace MangaScraper.Core.Scrapers.Manga {
  public delegate IProgress<double> GetProgress(string context);

  public interface IMetaDataRepository {
    //todo
    Task<(string name, MetaData metaData)[]> GetMetaData();
  }
}