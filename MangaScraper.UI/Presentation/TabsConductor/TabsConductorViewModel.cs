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
    protected override void OnActivate() {
      base.OnActivate();

    }
    public override void ActivateItem(IScreen s) {
      base.ActivateItem(s);
      s.Activate();
    }
    protected override void OnDeactivate(bool close) {
      base.OnDeactivate(close);
    }
    public bool IsCloseButtonVisible { get; set; }

    public void CloseItem(IScreen screen) { DeactivateItem(screen, true); }
  }
}