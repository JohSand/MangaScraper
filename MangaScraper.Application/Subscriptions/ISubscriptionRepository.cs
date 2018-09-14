using System.Collections.Generic;
using System.Threading.Tasks;

namespace MangaScraper.Application.Subscriptions {
    public interface ISubscriptionRepository {
        Task Save(SubscriptionItem item);

        Task<SubscriptionItem> GetSubscription(string name, string provider);
        Task<ICollection<SubscriptionItem>> GetSubscriptions();
    }
}