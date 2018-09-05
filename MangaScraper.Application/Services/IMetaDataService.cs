using System.Collections.Generic;
using MangaScraper.Core.Scrapers.Manga;
using System.Threading;
using System.Threading.Tasks;

namespace MangaScraper.Application.Services {
    public interface IMetaDataService {
        Task Start(string parser, CancellationToken token);

        GetProgress ReportProgressFactory { set; }

        ICollection<string> Parsers { get; }
    }
}