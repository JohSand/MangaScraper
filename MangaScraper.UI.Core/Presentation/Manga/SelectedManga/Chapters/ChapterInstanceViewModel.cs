using System.Collections.Generic;
using System.Linq;
using MangaScraper.Application.Services;
using MangaScraper.UI.Core.Helpers;
using MangaScraper.UI.Core.Presentation.Common;

namespace MangaScraper.UI.Core.Presentation.Manga.SelectedManga.Chapters
{
    public class ChapterInstanceViewModel : ChapterInstances
    {
        public ChapterInstanceViewModel(IEnumerable<ChapterInstance> chapters) : base(chapters) =>
            SelectedRows.CollectionChanged += (s, e) => NotifyOfPropertyChange(() => CanDownloadSelected);

        public bool CanDownloadSelected => SelectedRows?.Any() == true;

        public void DownloadSelected() => Download(SelectedRows);

        public void DownloadManga_All() => Download(Chapters);

        public void DownloadManga_OnClick(ChapterInstance chapter) => Download(new[] { chapter });

        private void Download(IEnumerable<ChapterInstance> chaptersToDownload)
        {
            var (targetFolder, canceled) = FolderDialog.GetTargetFolder();
            if (canceled) return;


            var a = chaptersToDownload
                .Select(c => (c.Number, (AsyncWork)(p => c.DownloadTo(targetFolder, p))))
                .Reverse()
                .ToList();
            MultiProgress.ScheduleProgress(a);
        }

        public MultiProgressViewModel MultiProgress { get; set; } =
            new MultiProgressViewModel
            {
                ErrorLog = (s, exception) => { }
            };
    }
}