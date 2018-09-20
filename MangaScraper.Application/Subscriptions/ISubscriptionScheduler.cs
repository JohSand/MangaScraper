using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MangaScraper.Application.Subscriptions {
    public interface ISubscriptionScheduler {
        Task Start(CancellationToken token);
        event EventHandler<NewChaptersEventArgs> NewChapters;
    }

    public class NewChaptersEventArgs : EventArgs {
        public string MangaName { get; }
        public ICollection<string> Chapters { get; }
        public NewChaptersEventArgs(string mangaName, ICollection<string> chapters) =>
             (MangaName, Chapters) = (mangaName, chapters);
    }
}