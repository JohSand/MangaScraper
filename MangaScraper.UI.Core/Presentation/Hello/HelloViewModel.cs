using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Caliburn.Micro;
using MangaScraper.Application.Services;
using MangaScraper.UI.Core.Composition;
using MangaScraper.UI.Core.Helpers;

namespace MangaScraper.UI.Core.Presentation.Hello
{
    public class HelloViewModel : Screen, IPrimaryScreen
    {
        private readonly CancellationTokenSource _source = new CancellationTokenSource();
        private readonly IMetaDataService _metaDataService;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly DispatcherTimer _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        public int Order => 2;

        public HelloViewModel(IMetaDataService metaDataService)
        {
            _metaDataService = metaDataService;
            Providers = _metaDataService.Parsers.ToBindableCollection();
            SelectedProvider = Providers.First();
            _timer.Tick += SetElapsed;
            this.OnPropertyChanges(s => s.Context).Subscribe(_ => _stopwatch.Restart());
        }

        private void SetElapsed(object _, EventArgs __) => ElapsedTime = _stopwatch.Elapsed.ToString(@"h\.mm\:ss");

        public BindableCollection<string> Providers { get; }

        public string SelectedProvider { get; set; }
        public string Context { get; set; }

        public double Progress { get; set; }

        public string ElapsedTime { get; set; }

        public Task Task { get; set; }

        protected override Task OnActivateAsync(CancellationToken token)
        {
            var dispatcher = Dispatcher.CurrentDispatcher;
            var progress = new Progress<double>(d => Progress = d * 100);
            _metaDataService.ReportProgressFactory = context =>
            {
                this.Context = SelectedProvider + ": " + context;

                if (!_timer.IsEnabled)
                    _timer.Start();
                return progress;
                //return new Progress<double>(d => dispatcher.Invoke(() =>  this.Test.Value = d));
            };
            return Task.CompletedTask;
        }



        public void StopTimer()
        {
            _timer.Stop();
            _stopwatch.Reset();
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            _metaDataService.ReportProgressFactory = null;
            _timer.Tick -= SetElapsed;
            _timer.Stop();
            return Task.CompletedTask;
        }

        public void Start() => Task = Task ?? _metaDataService.Start(SelectedProvider, _source.Token);

        public bool CanStart => Task is null;

        public void Stop()
        {
            _source.Cancel();
            try
            {
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
            get => "Indexing";
            set { }
        }


    }
}
