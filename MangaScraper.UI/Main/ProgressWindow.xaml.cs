using System;
using System.Windows;

namespace MangaScraper.UI.Main {
  /// <summary>
  /// Interaction logic for ProgressWindow.xaml
  /// </summary>
  public partial class ProgressWindow : Window, IDisposable {
    public ProgressWindow() => InitializeComponent();

    public void Update(double i) => ProgressBar.Value = i;

    public void Dispose() => Close();

    public IProgress<double> CreateProgress() => new Progress<double>(Update);
  }
}
