using MangaScraper.Core.Scrapers.Manga;
using System.Threading;
using System.Threading.Tasks;

namespace MangaScraper.Application.Services {
  public interface IMetaDataService {
    Task Start(CancellationToken token);

    GetProgress ReportProgressFactory { get; set; }
  }
}