using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using JetBrains.Annotations;

namespace MangaScraper.UI.Presentation.Common {
  public class MultiProgressViewModel {
    [UsedImplicitly]
    public int BatchSize { get; set; } = 3;

    public Action<string, Exception> ErrorLog { get; set; }

    [UsedImplicitly]
    public BindableCollection<ProgressListItem> Items { get; } = new BindableCollection<ProgressListItem>();

    public delegate Task UpdateInstruction(IProgress<double> d);

    public async void ScheduleProgress(IEnumerable<(string, UpdateInstruction)> thingsToDo) {
      var remainingItems = new Stack<(string, UpdateInstruction)>(thingsToDo);
      var size = Math.Min(BatchSize, remainingItems.Count);
      var currentItems = Enumerable.Range(0, size).Select(_ => CreateProgressBar(remainingItems)).ToList();

      while (currentItems.Any()) {
        var done = await Task.WhenAny(currentItems);
        currentItems.Remove(done);
        var item = await done;
        Items.Remove(item);
        if (item.Exception != null)
          ErrorLog?.Invoke(item.Name, item.Exception);

        if (remainingItems.Any() && currentItems.Count < BatchSize)
          currentItems.Add(CreateProgressBar(remainingItems));
      }
    }

    private Task<ProgressListItem> CreateProgressBar(Stack<(string, UpdateInstruction)> stack) {
      var (name, updateInstruction) = stack.Pop();
      var item = new ProgressListItem() { Name = $"Item {name}" };
      Items.Add(item);
      return updateInstruction(item.CreateProgress).ContinueWith(t => { item.Exception = t.Exception; return item; });
      //if removed with callback, i need to think about dispatcher threads   
    }

    public class ProgressListItem : INotifyPropertyChanged {
      [UsedImplicitly]
      public string Name { get; set; }
      [UsedImplicitly]
      public double Progress { get; set; }

      public Progress<double> CreateProgress => new Progress<double>(d => Progress = d);

      public event PropertyChangedEventHandler PropertyChanged;

      public AggregateException Exception { get; set; }
    }
  }
}