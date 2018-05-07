using System.Collections.Generic;
using MangaScraper.Core.Scrapers;

namespace MangaScraper.Application.Services {
  public class MangaInfo {
    public MetaData MetaData { get; set; }
    public string Name { get; set; }

    public List<(string provider, string url)> Instances { get; set; }
  }
}