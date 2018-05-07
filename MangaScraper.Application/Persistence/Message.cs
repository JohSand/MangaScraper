using MessagePack;

namespace MangaScraper.UI.Composition {
  [MessagePackObject]
  public struct Message {
    [Key(0)]
    public string Provider { get; set; }
    [Key(1)]
    public string Name { get; set; }
    [Key(2)]
    public string Url { get; set; }
  }
}
