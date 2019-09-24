using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace MangaScraper.UI.Core.Presentation.TabsConductor
{
    public class TabsConductorViewModel : Conductor<IScreen>.Collection.OneActive
    {
        public TabsConductorViewModel(IEnumerable<IScreen> screens)
        {
            Items.AddRange(screens);
        }

        //public void OpenTab() {
        //  ActivateItem(new HelloViewModel {
        //    DisplayName = "Tab "
        //  });
        //}

        public override async Task ActivateItemAsync(IScreen s, CancellationToken cancellationToken)
        {
            await base.ActivateItemAsync(s, cancellationToken);
            await s.ActivateAsync(cancellationToken);
        }

        public bool IsCloseButtonVisible { get; set; }

        public async void CloseItem(IScreen screen)
        {
            await DeactivateItemAsync(screen, true, CancellationToken.None);
        }
    }
}