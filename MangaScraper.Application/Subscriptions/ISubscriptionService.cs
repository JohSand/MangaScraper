using System.Collections.Generic;
using System.Threading.Tasks;

namespace MangaScraper.Application.Subscriptions {
    public interface ISubscriptionService {
        Task<List<string>> DownloadMissingChapters(SubscriptionItem item);
    }
}