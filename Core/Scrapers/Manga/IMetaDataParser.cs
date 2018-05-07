using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;

namespace MangaScraper.Core.Scrapers.Manga {
  public interface IMetaDataParser {
    /// <summary>
    /// The name of the page or site this parser is able to parse
    /// </summary>
    string ProviderName { get; }

    MetaData GetMetaData(IHtmlDocument doc);
    Task<IEnumerable<(string name, string url)>> ListInstances(PageGetter pageGetter, IProgress<double> progress);
  }
}