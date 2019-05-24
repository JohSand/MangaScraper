using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using JetBrains.Annotations;
using MangaScraper.UI.Helpers;

namespace MangaScraper.UI.Main {
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window, IDisposable {
        public ProgressWindow() => InitializeComponent();

        public void Dispose() => Close();

        private IReadOnlyDictionary<string, Progress<double>> Bars;

        public IProgress<double> GetProgress(string context) =>
            Bars.ContainsKey(context) ? Bars[context] : null;

        public void AddStacks(IReadOnlyCollection<string> mangaIndexProviders) {
            var coll = mangaIndexProviders.Select(s => new ProgressData {Name = s, Progress = 0.0d}).ToBindableCollection();

            Bars = mangaIndexProviders
                .Select((s, i) => (provider: s, index: i))
                .ToDictionary(t => t.provider, t => new Progress<double>(d => coll[t.index].Progress = d));

            DataView.ItemsSource = coll;
        }

        internal class ProgressData : INotifyPropertyChanged {
            public string Name { get; set; }
            public double Progress { get; set; }

            [UsedImplicitly]
            public event PropertyChangedEventHandler PropertyChanged;
        }
    }
}