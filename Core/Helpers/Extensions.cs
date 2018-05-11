using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MangaScraper.Core.Helpers {
  public static class Extensions {
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> a) {
      foreach (var x1 in source) a(x1);
    }

    public static void ForEach<T, X>(this IEnumerable<T> source, Func<T, X> a) {
      foreach (var x1 in source) a(x1);
    }

    public static async Task<IEnumerable<T>> SelectMany<X, T>(this IEnumerable<X> source, Func<X, Task<IEnumerable<T>>> func) {
      var all = await Task.WhenAll(source.Select(x => func(x)));
      return all.SelectMany(t => t);
    }

    public static async Task<IEnumerable<T>> SelectMany<X, T>(this IEnumerable<X> source, Func<X, Task<List<T>>> func) {
      var all = await Task.WhenAll(source.Select(x => func(x)));
      return all.SelectMany(t => t);
    }

    public static async Task<List<T>> ToListAsync<T>(this Task<IEnumerable<T>> source) {
      var result = await source.ConfigureAwait(false);
      return result is List<T> l ? l : result.ToList();
    }

    public static async Task<T[]> ToArrayAsync<T>(this Task<IEnumerable<T>> source) =>
      (await source.ConfigureAwait(false)).ToArray();

    public static IEnumerable<Task<T2>> AndThen<T, T2>(this IEnumerable<Task<T>> source, Func<T, Task<T2>> continuation) {
      return source.Select(t => t.ContinueWith(tt => TryDo(tt, continuation)).Unwrap());
    }

    public static IEnumerable<Task<T2>> AndThen<T, T2>(this IEnumerable<Task<T>> source, Func<T, T2> continuation) {
      return source.Select(t => t.ContinueWith(tt => TryDo(tt, continuation)));
    }

    public static Task<IEnumerable<T2>> Select<T, T2>(this Task<List<T>> source, Func<T, T2> continuation) {
      return source.ContinueWith(t => TryDo(t, x => x.Select(continuation)));
    }

    public static Task<IEnumerable<T2>> Select<T, T2>(this Task<T[]> source, Func<T, T2> continuation) {
      return source.ContinueWith(t => TryDo(t, x => x.Select(continuation)));
    }

    public static async Task<IEnumerable<T2>> ContinueWith<T, T2>(this IEnumerable<Task<T>> source, Func<T, Task<T2>> continuation) {
      return await Task.WhenAll(source.Select(t => t.ContinueWith(tt => TryDo(tt, continuation)).Unwrap())).ConfigureAwait(false);
    }

    /// <summary>
    /// Adds the continuation to each task in the source, and merges them to a single task, which completes when all tasks and continutations are done
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="source"></param>
    /// <param name="continuation"></param>
    /// <returns></returns>
    public static async Task<IEnumerable<T2>> ContinueWith<T, T2>(this IEnumerable<Task<T>> source, Func<T, T2> continuation) {
      var asdf = source.Select(t => t.ContinueWith(tt => TryDo(tt, continuation)));
      return await Task.WhenAll(asdf).ConfigureAwait(false);
    }

    public static async Task ContinueWith(this IEnumerable<Task> source, Action continuation) {
      await Task.WhenAll(source.Select(t => t.ContinueWith(tt => {
        if (tt.IsFaulted && tt.Exception != null)
          throw tt.Exception;
        continuation();
      }))).ConfigureAwait(false);
    }

    private static T2 TryDo<T1, T2>(Task<T1> tt, Func<T1, T2> action) =>
      tt.IsFaulted && tt.Exception != null
      ? throw tt.Exception
      : action(tt.Result);

    /// <summary>
    /// Flattens collections inside a task
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="task"></param>
    /// <returns></returns>
    public static async Task<IEnumerable<T>> Flatten<T>(this Task<IEnumerable<IEnumerable<T>>> task) {
      var result = await task.ConfigureAwait(false);
      return result.SelectMany(e => e);
    }

    public static async Task WhenAll(this IEnumerable<Task> values, IProgress<double> progress = null) {
      var arr = values.ToArray();
      var tempCount = 0;
      void Update() {
        progress?.Report(Interlocked.Increment(ref tempCount) / (double)arr.Length);
      }
      await arr.ContinueWith(Update).ConfigureAwait(false);
    }

    public static async Task<IEnumerable<T>> WhenAll<T>(this IEnumerable<Task<T>> values, IProgress<double> progress = null) {
      var arr = values.ToArray();
      var tempCount = 0;
      T Update(T value) {
        progress?.Report(Interlocked.Increment(ref tempCount) / (double)arr.Length);
        return value;
      }
      return await arr.ContinueWith(Update).ConfigureAwait(false);
    }
  }
}