using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using MangaScraper.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MangaScraper.Core.Scrapers.Manga.Eden {
    public struct SeriesParser : ISeriesParser, IMetaDataParser {
        public string ProviderName => "MangaEden";

        public MetaData GetMetaData(IHtmlDocument doc) {
            var metaData = new MetaData {
                Blurb = doc.GetElementById("mangaDescription").TextContent,
            };
            var rating = doc.GetElementById("rating");
            var author = rating.GetNextSiblingWithText("Author");
            metaData.Author = author?.NextElementSibling?.TextContent;

            var artist = (author ?? rating).GetNextSiblingWithText("Artist");
            metaData.Artist = artist?.NextElementSibling?.TextContent;

            var genres = (artist ?? rating).GetNextSiblingWithText("Genres");
            metaData.Genres = GetGenreATags(genres)
              .Select(e => e.TextContent)
              .Select(t => t.ParseAsGenre())
              .Where(t => t != Genre.None)
              .Merge();
            return metaData;
        }

        private IEnumerable<IElement> GetGenreATags(IElement genreTag) {
            if (genreTag == null) yield break;
            var next = genreTag.NextElementSibling;
            while (next != null && next.TextContent != "Type") {
                if (next.LocalName == "a")
                    yield return next;
                next = next.NextElementSibling;
            }
        }

        public async Task<IEnumerable<(string name, string url)>> ListInstances(PageGetter pageGetter, IProgress<double> progress = null) {
            var page = await pageGetter("https://www.mangaeden.com/en/en-directory/?page=1");
            var pag = page.GetElementsByClassName("pagination pagination_bottom").First();
            var count = pag.Children.Length;
            var secondLast = pag.Children[count - 2];
            var max = int.Parse(secondLast.TextContent);
            return await Enumerable.Range(1, max)
                .Select(i => $"https://www.mangaeden.com/en/en-directory/?page={i}")
                .Batch(8)
                .Transform(u => GetMangaList(pageGetter, u), progress);
        }



        private static async Task<IEnumerable<(string name, string url)>> GetMangaList(PageGetter doc, string url) {
            var page = await doc(url);
            return page
              .GetElementById("mangaList")
              .GetElementsByTagName("tbody")
              .First()
              .Elements("tr")
              .Select(tr => tr.Element("td").Element("a"))
              .Select(a => (a.TextContent, $"https://www.mangaeden.com{a.GetAttribute("href")}"));
        }

        public IEnumerable<string> ChapterUrls(IHtmlDocument doc) => doc.GetElementsByClassName("chapterLink").Select(a => $"https://www.mangaeden.com{a.GetAttribute("href")}").ToList();

        public string CoverUrl(IHtmlDocument doc) =>
      $"https:{doc.GetElementsByClassName("mangaImage2").First().Element("img").GetAttribute("src")}";

        public IChapterParser CreateChapter(string url) => new ChapterParser(url);
    }
}