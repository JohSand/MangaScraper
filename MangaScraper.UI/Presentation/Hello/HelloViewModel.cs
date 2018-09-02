using System;
using System.Windows.Threading;
using Caliburn.Micro;
using MangaScraper.Core.Scrapers.Manga;
using MangaScraper.UI.Composition;

namespace MangaScraper.UI.Presentation.Hello {
  public class HelloViewModel : Screen, IPrimaryScreen {
    private readonly IMetaDataService _metaDataService;
    public int Order => 2;

    public HelloViewModel(IMetaDataService metaDataService) {
      _metaDataService = metaDataService;

      Progress = 0.0;
    }

    public string Context { get; set; }

    public double Progress { get; set; }

    protected override void OnActivate() {
      base.OnActivate();
      var dispatcher = Dispatcher.CurrentDispatcher;
      var progress = new Progress<double>(d => Progress = d);
      //await Enumerable.Range(1, 100)
      //  .Select(async i => {
      //    dispatcher.Invoke(() => Test.Value = (double) i);
      //    await Task.Delay(10);
      //  })
      //  .WhenAll();
      _metaDataService.ReportProgressFactory = context => {
        this.Context = context;
        return progress;
        //return new Progress<double>(d => dispatcher.Invoke(() =>  this.Test.Value = d));
      };
    }

    protected override void OnDeactivate(bool close) {
      base.OnDeactivate(close);
      _metaDataService.ReportProgressFactory = null;
    }

    public bool? IsButtonVisible { get; set; }

    public override string DisplayName {
      get => "Hello";
      set { }
    }
  }
}
