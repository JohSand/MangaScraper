using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using JetBrains.Annotations;

namespace MangaScraper.UI.Presentation.Common {
    public delegate Task AsyncWork(IProgress<double> d);

    public class MultiProgressViewModel {
        [UsedImplicitly]
        public int BatchSize { get; set; } = 3;

        public Action<string, Exception> ErrorLog { get; set; }

        [UsedImplicitly]
        public BindableCollection<ProgressListItem> Items { get; } = new BindableCollection<ProgressListItem>();

        public async void ScheduleProgress(IEnumerable<(string, AsyncWork)> thingsToDo) {
            var remainingItems = new Stack<(string, AsyncWork)>(thingsToDo);
            var size = Math.Min(BatchSize, remainingItems.Count);
            var currentItems = new List<Task<ProgressListItem>>();
            foreach (var _ in Enumerable.Range(0, size)) {
                var (name, updateInstruction) = remainingItems.Pop();
                currentItems.Add(CreateProgressBar(name, updateInstruction));
            }

            while (currentItems.Any()) {
                var done = await Task.WhenAny(currentItems);
                currentItems.Remove(done);
                var item = await done;

                if (item.Exception != null)
                    ErrorLog?.Invoke(item.Name, item.Exception);

                if (remainingItems.Any() && currentItems.Count < BatchSize) {
                    var (name, updateInstruction) = remainingItems.Pop();
                    currentItems.Add(CreateProgressBar(name, updateInstruction));
                }
            }
        }

        private Task<ProgressListItem> CreateProgressBar(string name, AsyncWork work) {
            var item = new ProgressListItem() {Name = $"Chapter {name}"};
            Items.Add(item);
            return work(new Progress<double>(d => item.Progress = d))
                .ContinueWith(t => {
                        item.Exception = t.Exception;
                        Items.Remove(item);
                        return item;
                    },
                    TaskScheduler.FromCurrentSynchronizationContext());
            //if removed with callback, i need to think about dispatcher threads   
        }

        public class ProgressListItem : INotifyPropertyChanged {
            [UsedImplicitly]
            public string Name { get; set; }

            [UsedImplicitly]
            public double Progress { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;

            public AggregateException Exception { get; set; }
        }
    }
}