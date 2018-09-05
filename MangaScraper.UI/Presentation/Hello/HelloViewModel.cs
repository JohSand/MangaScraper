using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Caliburn.Micro;
using MangaScraper.Application.Services;
using MangaScraper.Core.Scrapers.Manga;
using MangaScraper.UI.Composition;
using MangaScraper.UI.Helpers;

namespace MangaScraper.UI.Presentation.Hello {
    public class HelloViewModel : Screen, IPrimaryScreen {
        private readonly CancellationTokenSource _source = new CancellationTokenSource();
        private readonly IMetaDataService _metaDataService;
        private Stopwatch stopWatch;
        private DispatcherTimer timer;

        public int Order => 2;

        public HelloViewModel(IMetaDataService metaDataService) {
            _metaDataService = metaDataService;
            Providers = _metaDataService.Parsers.ToBindableCollection();
            SelectedProvider = Providers.First();
            Progress = 0.0;
        }

        public BindableCollection<string> Providers { get; }

        public string SelectedProvider { get; set; }
        public string Context { get; set; }

        public double Progress { get; set; }

        public string ElapsedTime { get; set; }

        public Task Task { get; set; }

        protected override void OnActivate() {
            base.OnActivate();
            var dispatcher = Dispatcher.CurrentDispatcher;
            var progress = new Progress<double>(d => Progress = d);
            var timer = new DispatcherTimer();
            //await Enumerable.Range(1, 100)
            //  .Select(async i => {
            //    dispatcher.Invoke(() => Test.Value = (double) i);
            //    await Task.Delay(10);
            //  })
            //  .WhenAll();
            _metaDataService.ReportProgressFactory = context => {
                this.Context = SelectedProvider + ": " + context;
                StopTimer();
                StartTimer(timer);
                return progress;
                //return new Progress<double>(d => dispatcher.Invoke(() =>  this.Test.Value = d));
            };
        }

        public void StartTimer(DispatcherTimer timer) {
            this.timer = timer;
            stopWatch = new Stopwatch();
            timer.Tick += (s, e) => ElapsedTime = stopWatch.Elapsed.ToString(@"mm\:ss");
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1);

            stopWatch.Start();
            timer.Start();
        }

        public void StopTimer() {
            timer?.Stop();
            timer = null;
            stopWatch?.Stop();
            stopWatch = null;
        }


        protected override void OnDeactivate(bool close) {
            base.OnDeactivate(close);
            _metaDataService.ReportProgressFactory = null;
        }

        public void Start() => Task = Task ?? _metaDataService.Start(SelectedProvider, _source.Token);

        public bool CanStart => Task is null;

        public void Stop() {
            _source.Cancel();
            try {
                Task?.GetAwaiter().GetResult();
            }
            catch (Exception e) when (e is OperationCanceledException) { }

            Task = null;
            Context = "";
            Progress = 0;
            StopTimer();
        }

        public bool CanStop => Task != null;

        public bool? IsButtonVisible { get; set; }

        public override string DisplayName {
            get => "Hello";
            set { }
        }
    }
}