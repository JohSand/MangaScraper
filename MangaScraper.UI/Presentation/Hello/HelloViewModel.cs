using Caliburn.Micro;
using MangaScraper.Application.Services;
using MangaScraper.UI.Composition;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MangaScraper.UI.Presentation.Hello {
    public class HelloViewModel : Screen, IPrimaryScreen {
        private readonly CancellationTokenSource _source = new CancellationTokenSource();
        private readonly IMetaDataService _metaDataService;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly DispatcherTimer _timer = new DispatcherTimer {
            Interval = TimeSpan.FromSeconds(1)
        };

        public int Order => 2;

        public HelloViewModel(IMetaDataService metaDataService) => _metaDataService = metaDataService;

        private void SetElapsed(object _, EventArgs __) => ElapsedTime = _stopwatch.Elapsed.ToString("mm:ss");

        public string Context { get; set; }

        public double Progress { get; set; }

        public string ElapsedTime { get; set; }

        public Task Task { get; set; }

        protected override void OnActivate() {
            base.OnActivate();
            var progress = new Progress<double>(d => Progress = d);

            _metaDataService.ReportProgressFactory = context => {
                if (context != Context)
                    _stopwatch.Restart();

                this.Context = context;
                if (!_timer.IsEnabled)
                    _timer.Start();

                return progress;
            };
            _timer.Tick += SetElapsed;
        }

        public void StopTimer() {
            _timer.Stop();
            _stopwatch.Reset();
        }


        protected override void OnDeactivate(bool close) {
            base.OnDeactivate(close);
            _metaDataService.ReportProgressFactory = null;
            _timer.Tick -= SetElapsed;
            _timer.Stop();
        }
        public void Start() {
            //Task = Task ?? _metaDataService.Start(_source.Token);
            var callback = _metaDataService.ReportProgressFactory.Invoke("dummy");
        }

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
