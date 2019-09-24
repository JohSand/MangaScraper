using MangaScraper.Application.Services;
using MangaScraper.Core.Helpers;
using MangaScraper.Core.Scrapers.Manga;
using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace MangaScraper.Application
{

    public abstract class ParserServiceBase
    {
        private ParserServiceBase(PageGetter getter) =>
            PageGetter = getter;

        protected ParserServiceBase(IFileSystem fileSystem, PageGetter getter) : this(getter) =>
            FileSystem = fileSystem;

        private ParserServiceBase() =>
            PageGetter = Client.GetDocumentAsync;

        protected ParserServiceBase(IFileSystem fileSystem) : this() =>
            FileSystem = fileSystem;

        protected PageGetter PageGetter { get; }

        protected IFileSystem FileSystem { get; }

        public Task DownloadChapterTo(IChapterParser parser, string basePath, IProgress<double> progress = null)
        {
            var path = Path.Combine(basePath, parser.Number);
            if (!FileSystem.Directory.Exists(path))
                FileSystem.Directory.CreateDirectory(path);
            return WritePages(parser, path, progress);
        }

        public async Task WritePages(IChapterParser parser, string path, IProgress<double> progress)
        {
            var nrOfPages = await parser.GetPageCount(PageGetter);
            await Enumerable.Range(1, nrOfPages).Select(nr => WritePage(parser, path, nr)).WhenAll(progress);
        }

        public async Task WritePage(IChapterParser parser, string path, int nr)
        {
            var url = await parser.GetImageUrl(nr, PageGetter);
            var extension = url.Split('.').LastOrDefault()?.Split('?').FirstOrDefault();
            //todo
            await WriteFileToPath(url, $"{path}\\{parser.Number}_{nr}.{extension ?? "jpg"}");
        }

        public async Task WriteFileToPath(string url, string fileName)
        {
            using (var fs = FileSystem.File.OpenWrite(fileName))
            {
                await fs.DownloadToStream(url);
            }
        }
    }
}
