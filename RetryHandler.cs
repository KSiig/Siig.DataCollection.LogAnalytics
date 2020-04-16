using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Polly;

class RetryHandler : DelegatingHandler
{
  private const int MaxRetries = 5;

  public RetryHandler(HttpMessageHandler innerHandler)
   : base(innerHandler) { }

  protected override Task<HttpResponseMessage> SendAsync(
    HttpRequestMessage request,
    CancellationToken cancellationToken) =>
    Policy
      .Handle<HttpRequestException>()
      .Or<TaskCanceledException>()
      .OrResult<HttpResponseMessage>(x => !x.IsSuccessStatusCode)
      .WaitAndRetryAsync(MaxRetries, retryAttempt => TimeSpan.FromSeconds(Math.Pow(MaxRetries, retryAttempt)))
      .ExecuteAsync(() => base.SendAsync(request, cancellationToken));
}