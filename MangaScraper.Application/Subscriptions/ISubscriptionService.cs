using System.Collections.Generic;
using System.Threading.Tasks;
using MangaScraper.Core.Scrapers.Manga;

namespace MangaScraper.Application.Subscriptions {
    public interface ISubscriptionService {
        Task<List<string>> DownloadMissingChapters(SubscriptionItem item);
    }
}