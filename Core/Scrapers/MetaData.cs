namespace MangaScraper.Core.Scrapers {
  public struct MetaData {
    public string Author { get; set; }

    public string Artist { get; set; }

    public string Blurb { get; set; }

    public bool Completed { get; set; }

    public Genre Genres { get; set; }
  }
}