using System.Configuration;
using JetBrains.Annotations;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace MangaScraper.UI.Helpers {
  static class FolderDialog {
    [CanBeNull] private static string _defaultFolder;

    public static (string, bool) GetTargetFolder() {
      var dlg = FolderDialog.New(DefaultFoder);
      if (dlg.ShowDialog() != CommonFileDialogResult.Ok) return ("", true);

      DefaultFoder = dlg.FileName;
      return (DefaultFoder, false);
    }

    public static CommonOpenFileDialog New(string defaultFoder) =>
      new CommonOpenFileDialog {
        Title = "Select Download Directory",
        IsFolderPicker = true,
        InitialDirectory = defaultFoder,
        DefaultDirectory = defaultFoder,
        AddToMostRecentlyUsedList = false,
        AllowNonFileSystemItems = false,
        EnsureFileExists = true,
        EnsurePathExists = true,
        EnsureReadOnly = false,
        EnsureValidNames = true,
        Multiselect = false,
        ShowPlacesList = true
      };

    public static string DefaultFoder {
      get {
        if (_defaultFolder != null) return _defaultFolder;

        var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        _defaultFolder = config.AppSettings.Settings["defaultDirectory"].Value;
        return _defaultFolder;
      }
      set {
        if (_defaultFolder == value) return;
        // To Save
        var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        config.AppSettings.Settings["defaultDirectory"].Value = value;
        config.AppSettings.SectionInformation.ForceSave = true;
        config.Save(ConfigurationSaveMode.Modified);

        _defaultFolder = value;
      }
    }
  }
}