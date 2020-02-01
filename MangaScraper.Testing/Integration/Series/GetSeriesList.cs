using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MangaScraper.Application.Services;
using MangaScraper.Core.Helpers;
using MangaScraper.Core.Scrapers.Manga;
using Xunit;

namespace MangaScraper.Testing.Integration.Series
{
    public class GetSeriesList
    {
        [Fact]
        public async Task CanListAllEden() {
            var k = new MangaScraper.Core.Scrapers.Manga.Eden.SeriesParser();
            var docs = await k.ListInstances(Client.GetDocumentAsync, new Progress<double>());
            

            Assert.NotEmpty(docs);
        }

        [Fact]
        public async Task EdenHandlesNotFound() {
            ISeriesParser k = new MangaScraper.Core.Scrapers.Manga.Eden.SeriesParser();
            var naruto = k.CreateChapter("https://www.mangaeden.com/en/en-manga/naruto/");
            var docs = await naruto.GetPageCount(Client.GetDocumentAsync);
            var page = await Client.GetDocumentAsync("https://www.mangaeden.com/en/en-manga/naruto/");
            var urls = k.ChapterUrls(page).ToList();
            Assert.Equal(0, docs);
            Assert.Empty(urls);

        }
    }
}
