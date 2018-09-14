using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Parser.Html;
using MangaScraper.Application.Services;
using MangaScraper.Core.Helpers;
using MangaScraper.Core.Scrapers;
using MangaScraper.Core.Scrapers.Manga;
using Xunit;

namespace MangaScraper.Testing.Integration.Metadata {
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
        public void TestMe() {
            var parser = new HtmlParser();
           
            var list = new List<EventArgs>();
            parser.Context.ParseError += (sender, ev) => list.Add(ev);

            var doc = parser.Parse("error");
            Assert.NotEmpty(list);
        }

        [Fact]
        public void AllNumbers() {
            var k = new MangaScraper.Core.Scrapers.Manga.Kakalot.SeriesParser();
            var di = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "TestData")).GetFiles()
                .First(f => f.Name.Contains("html"));
            var parser = new HtmlParser();

            using (var s = di.OpenRead()) {
                var doc = parser.Parse(s);
                var d = k.GetMetaData(doc);
                Assert.NotNull(d);
                Assert.Equal(Genre.Romance | Genre.SchoolLife | Genre.Shoujo, d.Genres);
                Assert.Equal("Fujisaki Mao", d.Author);
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
                            var d = k.GetMetaData(await Client.GetDocumentAsync(t.url));
                            Assert.NotNull(d);
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