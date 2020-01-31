using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using MangaScraper.Core.Helpers;

namespace MangaScraper.Core.Scrapers.Manga.Kakalot {
    public struct SeriesParser : ISeriesParser, IMetaDataParser {
        public string ProviderName => "MangaKakalot";

        public MetaData GetMetaData(IHtmlDocument doc) {
            var info = doc.GetElementsByClassName("manga-info-text").First();
            var li = info.Elements("li").ToList();
            var author = li[1];
            var genres = li[6];
            var strings = genres.Elements("a").Select(e => e.TextContent.ParseAsGenre()).Merge();
            var blurb = doc.GetElementById("noidungm");
            return new MetaData {
                Genres = strings,
                Author = author.Elements("a").First().TextContent,
                Completed = li[2].TextContent != "Status : Ongoing",
                Blurb = blurb.ChildNodes.Last().TextContent.Trim()
            };
        }


        public async Task<IEnumerable<(string name, string url)>> ListInstances(PageGetter pageGetter, IProgress<double> progress = null) {
            var doc = await pageGetter("http://mangakakalot.com/manga_list?type=topview&category=all&state=all&page=1");
            var groupPage = doc.GetElementsByClassName("group_page").First();
            var last = groupPage.Children.Last(c => c.LocalName == "a").TextContent.Replace("Last(", "").Replace(")", "");
            var max = int.Parse(last);
            return await Enumerable.Range(1, max)
                .Select(i => $"http://mangakakalot.com/manga_list?type=topview&category=all&state=all&page={i}")
                .Batch(20)
                .Transform(u => GetForUrl(pageGetter, u), progress);


            //todo this seems heavy...
            //return await Enumerable.Range(1, index)
            //  .Select(i => $"http://mangakakalot.com/manga_list?type=topview&category=all&state=all&page={i}")
            //  .Select(url => GetForUrl(pageGetter, url))
            //  .WhenAll(progress)
            //  .Flatten()
            //  .ToListAsync();
        }

        private static async Task<IEnumerable<(string, string)>> GetForUrl(PageGetter getter, string url) {
            var index = await getter(url).ConfigureAwait(false);
            return index
                .GetElementsByClassName("list-truyen-item-wrap")
                .Select(wrap => wrap.Element("a"))
                .Select(a => {
                    var href = a.GetAttribute("href");
                    var title = a.GetAttribute("title");
                    return (title, href);
                })
                .ToList();
        }

        public IEnumerable<string> ChapterUrls(IHtmlDocument doc) {
            var chapterList = doc.GetElementsByClassName("chapter-list").FirstOrDefault();
            if (chapterList is {})
            {
                return chapterList.Elements("div").Select(d => d.Element("span").Element("a").GetAttribute("href")).ToList();
            }
            else
            {
                var c = doc.GetElementsByClassName("row-content-chapter").First();
                return c.Elements("li").Select(d => d.Element("a").GetAttribute("href")).ToList();
            }
        }

        public string CoverUrl(IHtmlDocument doc) {
            var pic = doc.GetElementsByClassName("manga-info-pic").FirstOrDefault();
            if (pic is {})
            {
                var img = pic.Element("img");
                var src = img.GetAttribute("src");
                return src;
            }
            else
            {
                var p = doc.GetElementsByClassName("info-image").First();
                var src = p.Element("img").GetAttribute("src");
                return src;
            }
        }

        public IChapterParser CreateChapter(string url) => new ChapterParser(url);
    }
}