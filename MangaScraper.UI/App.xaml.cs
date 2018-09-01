using System;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace MangaScraper.UI {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application {
        protected override void OnStartup(StartupEventArgs e) {
            //Dispatcher.UnhandledException += AppDomainOnUnhandledException;
            base.OnStartup(e);
        }

        private void AppDomainOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args) {
            var errorMessage = GetErrorText(args.Exception);
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        }

        private static string GetErrorText(Exception ex) {
            string FormatErrorMessage(Exception exception) => $"Exception: {exception.GetType()}"
                                                              + Environment.NewLine + Environment.NewLine +
                                                              $"{exception.Message}"
                                                              + Environment.NewLine + Environment.NewLine +
                                                              $"{exception.StackTrace}"
                                                              + Environment.NewLine + Environment.NewLine;

            string GetErrorTextRec(Exception e, StringBuilder message) {
                if (e == null) return message.ToString();
                string errorMessage = FormatErrorMessage(ex);
                message.Append(errorMessage);
                return GetErrorTextRec(e.InnerException, message);
            }

            var sb = new StringBuilder($"An unhandled exception occurred:");
            return GetErrorTextRec(ex, sb);
        }
    }
}