using Caliburn.Micro;

namespace MangaScraper.UI.Core.Composition
{
    public interface IPrimaryScreen : IScreen
    {
        int Order { get; }
    }
}