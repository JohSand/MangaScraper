using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Reflection;
using System.Windows;
using Autofac;
using MangaScraper.Application.Services;
using MangaScraper.Core.Scrapers.Manga;
using MangaScraper.UI.Composition;
using MangaScraper.UI.Presentation.Shell;

namespace MangaScraper.UI.Bootstrap {
  public class Bootstrapper : AutofacBootstrapper<ShellViewModel> {

    public Bootstrapper() { Initialize(); }

    protected override void ConfigureBootstrapper() {
      base.ConfigureBootstrapper();
      EnforceNamespaceConvention = false;
    }

    protected override void ConfigureContainer(ContainerBuilder builder) {
      var dataAccess = Assembly.GetExecutingAssembly();
      builder.RegisterAssemblyTypes(dataAccess)
        .Where(t => t.Name.EndsWith("ViewModel"))
        .AsImplementedInterfaces()
        .AsSelf();



      builder.Register(_ => new MangaScraper.Core.Scrapers.Manga.Eden.SeriesParser()).AsImplementedInterfaces();
      builder.Register(_ => new MangaScraper.Core.Scrapers.Manga.Panda.SeriesParser()).AsImplementedInterfaces();
      builder.Register(_ => new MangaScraper.Core.Scrapers.Manga.Fun.SeriesParser()).AsImplementedInterfaces();
      builder.Register(_ => new MangaScraper.Core.Scrapers.Manga.Kakalot.SeriesParser()).As<ISeriesParser>();
      //builder.RegisterType<FoxScraper>().AsImplementedInterfaces();
      //builder.RegisterType<MangaFoxProvider>().AsImplementedInterfaces().SingleInstance();
      //todo register parser by convention
      builder.RegisterType<FileSystem>().AsImplementedInterfaces().SingleInstance();
      builder.RegisterType<MangaDownloader>().AsImplementedInterfaces().SingleInstance();
      builder.RegisterType<MangaIndex>().AsImplementedInterfaces().SingleInstance();
      builder.RegisterType<MemFile>().AsImplementedInterfaces().SingleInstance();
      builder.RegisterType<MetaDataService>().AsImplementedInterfaces().SingleInstance();
    }


    protected override IEnumerable<object> GetAllInstances(Type serviceType) => base.GetAllInstances(serviceType);

    protected override void BuildUp(object instance) { base.BuildUp(instance); }

    protected override void OnStartup(object sender, StartupEventArgs e) {
      DisplayRootViewFor<ShellViewModel>(new Dictionary<string, object> {
        ["WindowState"] = WindowState.Maximized,
        ["SizeToContent"] = SizeToContent.Manual
      });
    }

    protected override void OnExit(object sender, EventArgs e) {
      var asd = Container.Resolve<IMangaIndex>();
      asd.Stop();
      base.OnExit(sender, e);
    }
  }
}