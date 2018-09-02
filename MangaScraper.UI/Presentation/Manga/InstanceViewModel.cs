using System.Windows.Media.Imaging;
using Caliburn.Micro;
using MangaScraper.Core.Scrapers;

namespace MangaScraper.UI.Presentation.Manga {
    public class InstanceViewModel : PropertyChangedBase {
        public MetaData MetaData { get; set; }

        public string Name { get; set; }

        //todo genres, description, etc

        public ChapterInstanceViewModel ChapterInstanceViewModel { get; set; }

        public BitmapImage Cover { get; set; }

        public string Genres => MetaData.Genres.ToString();

        public string Blurb => MetaData.Blurb;
    }
}