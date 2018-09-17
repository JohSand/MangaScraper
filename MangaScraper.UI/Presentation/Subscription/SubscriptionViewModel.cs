using Caliburn.Micro;
using MangaScraper.Application.Subscriptions;
using MangaScraper.UI.Composition;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MangaScraper.UI.Presentation.Subscription {
    public class SubscriptionViewModel : Screen, IPrimaryScreen {
        public int Order => 3;
        private readonly CancellationTokenSource _source = new CancellationTokenSource();
        private readonly ISubscriptionScheduler _scheduler;
        private Task Task { get; set; }

        public BindableCollection<UpdateInfo> Updates { get; } = new BindableCollection<UpdateInfo>();

        public SubscriptionViewModel(ISubscriptionScheduler scheduler) {
            _scheduler = scheduler;
            _scheduler.NewChapters += (o, e) => {
                foreach (var u in e.Chapters) {
                    if (Updates.Count > 19) {
                        Updates.RemoveAt(0);
                    }
                    Updates.Add(new UpdateInfo {
                        Added = DateTime.UtcNow,
                        Chapter = u,
                        Series = e.MangaName
                    });
                }
            };
        }

        public override string DisplayName {
            get => "Subscription";
            set { }
        }

        public void Start() => Task = Task ?? _scheduler.Start(_source.Token);

        public bool CanStart => Task is null;

        public void Stop() {
            _source.Cancel();
            try {
                Task?.GetAwaiter().GetResult();
            }
            catch (Exception e) when (e is TaskCanceledException) { }
            Task = null;
        }

        public bool CanStop => Task != null;
    }

    public struct UpdateInfo {
        public string Series { get; set; }
        public string Chapter { get; set; }
        public DateTime Added { get; set; }
    }
}