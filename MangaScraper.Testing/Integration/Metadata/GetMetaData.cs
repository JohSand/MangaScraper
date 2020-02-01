using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using MangaScraper.Application.Services;
using MangaScraper.Core.Helpers;
using MangaScraper.Core.Scrapers;
using MangaScraper.Core.Scrapers.Manga;
using Xunit;

namespace MangaScraper.Testing.Integration.Metadata {
    public class MangaFailedException : Exception
    {
        public MangaFailedException(string empty) : base(empty)
        {
            
        }
    }

    public class GetMetaData {
        //public static IEnumerable<object[]> GetNumbers() {
        //  return System.AppDomain.CurrentDomain.GetAssemblies()
        //    .SelectMany(s => s.GetTypes())
        //    .Where(p => typeof(IMetaDataParser).IsAssignableFrom(p) && !p.IsInterface)
        //    .Select(t => new [] {Activator.CreateInstance(t)})
        //    .ToList();
        //}

        public bool IsOddNumber(int number) {
            return number % 2 != 0;
        }

        [Fact]
        public void Test1() {
            var k = new MangaScraper.Core.Scrapers.Manga.Kakalot.SeriesParser();
            var di = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "TestData")).GetFiles()
                .First(f => f.Name.Contains("kakalot1.html"));
            var parser = new HtmlParser();

            using (var s = di.OpenRead()) {
                var doc = parser.ParseDocument(s);
                var d = k.GetMetaData(doc);
                Assert.Equal(Genre.Romance | Genre.SchoolLife | Genre.Shoujo, d.Genres);
                Assert.Equal("Fujisaki Mao", d.Author);
                Assert.NotEmpty(d.Blurb);
            }
        }

        [Fact]
        public void Test2() {
            var k = new MangaScraper.Core.Scrapers.Manga.Kakalot.SeriesParser();
            var di = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "TestData")).GetFiles()
                .First(f => f.Name.Contains("kakalot2.html"));
            var parser = new HtmlParser();

            using (var s = di.OpenRead()) {
                var doc = parser.ParseDocument(s);
                var d = k.GetMetaData(doc);
                Assert.Equal("Inoue Sora", d.Author);
                Assert.NotEmpty(d.Blurb);
                Assert.Contains("Kunimitsu", d.Blurb);
            }
        }

        [Fact]
        public void Test3() {
            var k = new MangaScraper.Core.Scrapers.Manga.Kakalot.SeriesParser();
            var di = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "TestData")).GetFiles()
                .First(f => f.Name.Contains("kakalot3.html"));
            var parser = new HtmlParser();

            using (var s = di.OpenRead()) {
                var doc = parser.ParseDocument(s);
                var d = k.GetMetaData(doc);
                Assert.Equal("OOKUBO Atsushi", d.Author);
                Assert.NotEmpty(d.Blurb);
            }
        }

        [Fact]
        public void TestEden1() {
            var k = new MangaScraper.Core.Scrapers.Manga.Eden.SeriesParser();
            var di = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "TestData")).GetFiles()
                .First(f => f.Name.Contains("eden1.html"));
            var parser = new HtmlParser();

            using (var s = di.OpenRead()) {
                var doc = parser.ParseDocument(s);
                var d = k.GetMetaData(doc);
                Assert.Equal("OHKUBO Atsushi", d.Author);
                Assert.NotEmpty(d.Blurb);
            }
        }

        [Fact]
        public async Task CanParseAllKakalot() {
            IMetaDataParser k = new MangaScraper.Core.Scrapers.Manga.Kakalot.SeriesParser();
            var docs = await k.ListInstances(Client.GetDocumentAsync, new Progress<double>());
            var faiList = new List<string>();
            await docs.Batch(50)
                .Transform(async t => {
                        try {
                            k.GetMetaData(await Client.GetDocumentAsync(t.url));
                        }
                        catch (Exception e)
                        {
                            throw new MangaFailedException("failed for url: " + t.url);
                        }

                        return 0;
                    },
                    CancellationToken.None,
                    null,
                    0);

            Assert.Empty(faiList);
        }

        [Fact]
        public async Task CanParseAllEden() {
            IMetaDataParser k = new MangaScraper.Core.Scrapers.Manga.Eden.SeriesParser();
            var docs = await k.ListInstances(Client.GetDocumentAsync, new Progress<double>());
            var faiList = new List<string>();
            await docs.Batch(50)
                .Transform(async t => {
                        try {
                            k.GetMetaData(await Client.GetDocumentAsync(t.url));
                        }
                        catch (Exception e) {
                            Console.WriteLine(e);
                            faiList.Add(t.url);
                        }

                        return 0;
                    },
                    CancellationToken.None,
                    null,
                    0);

            Assert.Empty(faiList);
        }


        [Fact]
        public async Task TestBrawlingGo()
        {
            var url = "https://manganelo.com/manga/brawling_go";
            IMetaDataParser k = new MangaScraper.Core.Scrapers.Manga.Kakalot.SeriesParser();
            var meta = k.GetMetaData(await Client.GetDocumentAsync(url));

            Assert.Equal("Worin", meta.Author);
            Assert.Equal(
                Genre.Adult | Genre.Comedy | Genre.Ecchi | Genre.Romance | Genre.Supernatural
                , meta.Genres);
            Assert.True(meta.Completed);
        }

        [Fact]
        public async Task TestCultivationChatGroup()
        {
            var url = "https://manganelo.com/manga/ulma274311576209244";
            IMetaDataParser k = new MangaScraper.Core.Scrapers.Manga.Kakalot.SeriesParser();
            var meta = k.GetMetaData(await Client.GetDocumentAsync(url));

            Assert.Equal("Legend Of The Sacred Knight", meta.Author);
            Assert.Equal(
                Genre.Action | Genre.Comedy | Genre.MartialArts | Genre.SchoolLife
                , meta.Genres);
            Assert.False(meta.Completed);
        }

        [Fact]
        public async Task TestHardcoreLevelingWarrior()
        {
            var url = "https://manganelo.com/manga/wdaq187991567387650";
            IMetaDataParser k = new MangaScraper.Core.Scrapers.Manga.Kakalot.SeriesParser();
            var meta = k.GetMetaData(await Client.GetDocumentAsync(url));

            Assert.Equal("Updating", meta.Author);
            Assert.Equal(
                Genre.None
                , meta.Genres);
            Assert.False(meta.Completed);
        }

        [Fact]
        public async Task TestArgateonline()
        {
            var url = "https://mangakakalot.com/manga/argate_online";
            IMetaDataParser k = new MangaScraper.Core.Scrapers.Manga.Kakalot.SeriesParser();
            var meta = k.GetMetaData(await Client.GetDocumentAsync(url));

            Assert.Equal("Updating", meta.Author);
            Assert.Equal(
                Genre.Action | Genre.Adventure | Genre.Ecchi | Genre.Fantasy |
                Genre.MartialArts | Genre.Shounen | Genre.Supernatural
                , meta.Genres);
            Assert.False(meta.Completed);
        }

        [Fact]
        public async Task TestBurakkuGakkou()
        {
            var url = "https://manganelo.com/manga/yydw283041580202318";
            IMetaDataParser k = new MangaScraper.Core.Scrapers.Manga.Kakalot.SeriesParser();
            var meta = k.GetMetaData(await Client.GetDocumentAsync(url));

            Assert.Equal("Souryuu", meta.Author);
            Assert.Equal(
                Genre.Comedy | Genre.SchoolLife | Genre.Ecchi | Genre.Seinen 
                , meta.Genres);
            Assert.False(meta.Completed);
        }

        //[Fact]
        //public async Task CanParseAllEden() {
        //    IMetaDataParser k = new MangaScraper.Core.Scrapers.Manga.Eden.SeriesParser();
        //    var docs = await k.ListInstances(Client.GetDocumentAsync, new Progress<double>());
        //    var faiList = new List<string>();
        //    await docs.Batch(50)
        //        .Transform(async t => {
        //                try {
        //                    var d = k.GetMetaData(await Client.GetDocumentAsync(t.url));
        //                    Assert.NotNull(d);
        //                }
        //                catch (Exception e) {
        //                    Console.WriteLine(e);
        //                    faiList.Add(t.url);
        //                }

        //                return 0;
        //            },
        //            CancellationToken.None,
        //            null,
        //            0);

        //    Assert.Empty(faiList);            
        //}
    }
}