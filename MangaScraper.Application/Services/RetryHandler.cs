using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MangaScraper.Application.Services {
  public class RetryHandler : DelegatingHandler {
    // Strongly consider limiting the number of retries - "retry forever" is
    // probably not the most user friendly way you could respond to "the
    // network cable got pulled out."
    private const int MaxRetries = 3;

    public RetryHandler() : base(new HttpClientHandler()) { }

    public RetryHandler(HttpMessageHandler innerHandler)
      : base(innerHandler) { }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
      var counter = MaxRetries;
      while (true) {
        try {
          var res = await base.SendAsync(request, cancellationToken);
          res.EnsureSuccessStatusCode();
          return res;
        }
        catch (HttpRequestException) when (counter > 0) {
          counter = counter - 1;
          await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, 5 - counter)), cancellationToken);
        }
      }
    }
  }
}