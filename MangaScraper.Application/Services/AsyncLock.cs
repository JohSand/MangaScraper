using System;
using System.Threading;
using System.Threading.Tasks;

namespace MangaScraper.Application.Services {
  public sealed class AsyncLock {
    private SemaphoreSlim Semaphore { get; } = new SemaphoreSlim(1, 1);
    private Task<Releaser> Key { get; }

    public AsyncLock() => Key = Task.FromResult(new Releaser(this));

    public Task<Releaser> LockAsync() {
      var lockTask = Semaphore.WaitAsync();
      return lockTask.IsCompleted
        ? Key
        : lockTask.ContinueWith(
          continuationFunction: (_, state) => new Releaser((AsyncLock)state),
          state: this,
          continuationOptions: TaskContinuationOptions.ExecuteSynchronously);
    }

    public struct Releaser : IDisposable {
      private AsyncLock Lock { get; }
      internal Releaser(AsyncLock @lock) => Lock = @lock;

      public void Dispose() => Lock.Semaphore.Release();
    }
  }
}