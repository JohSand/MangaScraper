using JetBrains.Annotations;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace MangaScraper.UI.Helpers {
    internal static class FolderDialog {
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

                _defaultFolder = Properties.Settings.Default.defaultDirectory;
                return _defaultFolder;
            }
            set {
                if (_defaultFolder == value) return;
                // To Save
                Properties.Settings.Default.defaultDirectory = value;
                Properties.Settings.Default.Save();
                _defaultFolder = value;
            }
        }
    }
}