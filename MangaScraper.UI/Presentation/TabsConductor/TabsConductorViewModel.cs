using System.Collections.Generic;
using Caliburn.Micro;

namespace MangaScraper.UI.Presentation.TabsConductor {
  public class TabsConductorViewModel : Conductor<IScreen>.Collection.OneActive {
    public TabsConductorViewModel(IEnumerable<IScreen> screens) {
      Items.AddRange(screens);
    }

    //public void OpenTab() {
    //  ActivateItem(new HelloViewModel {
    //    DisplayName = "Tab "
    //  });
    //}

    public bool IsCloseButtonVisible { get; set; }

    public void CloseItem(IScreen screen) { DeactivateItem(screen, true); }
  }
}