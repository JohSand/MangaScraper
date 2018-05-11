using System;
using System.Threading;
using System.Threading.Tasks;

namespace MangaScraper.Core.Scrapers.Manga {
  public delegate IProgress<double> GetProgress(string context);

  public interface IMetaDataService {
    //todo
    Task<(string name, MetaData metaData)[]> GetMetaData();

    Task Start(CancellationToken token);

    GetProgress ReportProgressFactory { set; }
  }
}