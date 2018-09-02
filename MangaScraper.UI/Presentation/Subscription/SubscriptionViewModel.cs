using Caliburn.Micro;
using MangaScraper.UI.Composition;

namespace MangaScraper.UI.Presentation.Subscription {
    public class SubscriptionViewModel : Screen, IPrimaryScreen {
        public int Order => 3;

        public override string DisplayName {
            get => "Subscription";
            set { }
        }
    }
}