using Autofac;
using BespokeFusion;
using MangaScraper.Application.Persistence;
using MangaScraper.Application.Services;
using MangaScraper.Application.Subscriptions;
using MangaScraper.UI.Presentation.Shell;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;

namespace MangaScraper.UI.Bootstrap {
    public class Bootstrapper : AutofacBootstrapper<ShellViewModel> {
        public Bootstrapper() => Initialize();

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

            //todo test



            builder.Register(_ => new Core.Scrapers.Manga.Eden.SeriesParser()).AsImplementedInterfaces();
            builder.Register(_ => new Core.Scrapers.Manga.Panda.SeriesParser()).AsImplementedInterfaces();
            builder.Register(_ => new Core.Scrapers.Manga.Fun.SeriesParser()).AsImplementedInterfaces();
            builder.Register(_ => new Core.Scrapers.Manga.Kakalot.SeriesParser()).AsImplementedInterfaces();
            //builder.RegisterType<FoxScraper>().AsImplementedInterfaces();
            //builder.RegisterType<MangaFoxProvider>().AsImplementedInterfaces().SingleInstance();
            //todo register parser by convention
            builder.RegisterType<FileSystem>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<MangaDownloader>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<MangaIndex>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<MemFile>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<MetaDataService>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<SubscriptionRepository>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<SubscriptionService>().AsImplementedInterfaces();
            builder.RegisterType<SubscriptionScheduler>().AsImplementedInterfaces().SingleInstance();



            AppDomain.CurrentDomain.UnhandledException += AppDomainOnUnhandledException;
        }

        private void AppDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs args) {
            if (args.ExceptionObject is Exception exception) {
                var errorMessage = GetErrorText(exception);
                MaterialMessageBox.Show(errorMessage, "Error");
                //exception.Handled = true;
            }
        }

        private static string GetErrorText(Exception ex) {
            string FormatErrorMessage(Exception exception) => $"Exception: {exception.GetType()}"
                                                              + Environment.NewLine + Environment.NewLine +
                                                              $"{exception.Message}"
                                                              + Environment.NewLine + Environment.NewLine +
                                                              $"{exception.StackTrace}"
                                                              + Environment.NewLine + Environment.NewLine;

            var sb = new StringBuilder($"An unhandled exception occurred:");
            var str = GetExceptions(ex).Aggregate(sb, (agg, curr) => agg.Append(FormatErrorMessage(curr)));
            return str.ToString();
        }

        public static IEnumerable<Exception> GetExceptions(Exception e) {
            while (true) {
                if (e is null) yield break;
                yield return e;
                e = e.InnerException;
            }
        }


        protected override IEnumerable<object> GetAllInstances(Type serviceType) => base.GetAllInstances(serviceType);

        protected override void BuildUp(object instance) => base.BuildUp(instance);

        protected override void OnStartup(object sender, StartupEventArgs e) => DisplayRootViewFor<ShellViewModel>(new Dictionary<string, object> {
            ["WindowState"] = WindowState.Maximized,
            ["SizeToContent"] = SizeToContent.Manual
        });

        protected override void OnExit(object sender, EventArgs e) => base.OnExit(sender, e);
    }
}