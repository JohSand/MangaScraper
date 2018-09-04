using System;
using System.Collections.Generic;
using System.Linq;
using MangaScraper.Core.Scrapers.Manga;
using Xunit;

namespace MangaScraper.Testing.Integration.Metadata {
  public class GetMetaData {
    public static IEnumerable<object[]> GetNumbers() {
      return System.AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(s => s.GetTypes())
        .Where(p => typeof(IMetaDataParser).IsAssignableFrom(p) && !p.IsInterface)
        .Select(t => new [] {Activator.CreateInstance(t)})
        .ToList();
    }

    public bool IsOddNumber(int number) {
      return number % 2 != 0;
    }

    [Theory]
    [MemberData(nameof(GetNumbers))]
    public void AllNumbers(IMetaDataParser a) {
      Assert.NotNull(a);
      a.ListInstances()
    }
  }
}
