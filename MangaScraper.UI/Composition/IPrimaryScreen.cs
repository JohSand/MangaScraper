using Caliburn.Micro;

namespace MangaScraper.UI.Composition {
  public interface IPrimaryScreen : IScreen{
    int Order { get; }
  }
}