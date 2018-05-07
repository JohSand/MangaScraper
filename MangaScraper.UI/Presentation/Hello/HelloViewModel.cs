using Caliburn.Micro;
using MangaScraper.UI.Composition;

namespace MangaScraper.UI.Presentation.Hello {
  public class HelloViewModel : Screen, IPrimaryScreen {

    public int Order => 2;



    public bool? IsButtonVisible { get; set; }

    public override string DisplayName {
      get => "Hello";
      set { }
    }
  }
}
