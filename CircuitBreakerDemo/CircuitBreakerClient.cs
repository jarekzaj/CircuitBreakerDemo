using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace CircuitBreaker
{
    public interface ICircuitBreakerClient
    {
        Task<IActionResult> ExecuteCircuitBreakerPolicy(int? timeout);
    }

    public class CircuitBreakerClient : ICircuitBreakerClient
    {
        private readonly AsyncCircuitBreakerPolicy<IActionResult> _circuitBreaker;
        private readonly HttpClient _httpClient;
        private readonly AsyncPolicy<IActionResult> _policy;

        private const int CircuitBreakerDurationOfBreak = 40;
        private const int CircuitBreakerEventsBeforeBreaking = 2;
        private const int PolicyTimeout = 10;

        public CircuitBreakerClient(IHttpClientFactory clientFactory)
        {
            _httpClient = clientFactory.CreateClient();

            _circuitBreaker = Policy<IActionResult>
                .Handle<Exception>()
                .CircuitBreakerAsync(
                    CircuitBreakerEventsBeforeBreaking,
                    TimeSpan.FromSeconds(CircuitBreakerDurationOfBreak)
                );

            var timeoutPolicy = Policy.TimeoutAsync(PolicyTimeout, TimeoutStrategy.Pessimistic);

            _policy = Policy<IActionResult>
                .Handle<Exception>()
                .FallbackAsync(async cancellationToken => ExecuteCircuitBreakerFallback())
                .WrapAsync(_circuitBreaker)
                .WrapAsync(timeoutPolicy);
        }

        public async Task<IActionResult> ExecuteCircuitBreakerPolicy(int? timeout)
        {
            return await _policy.ExecuteAsync(() => CallCircuitBreakerApi(timeout));
        }

        public async Task<IActionResult> CallCircuitBreakerApi(int? timeout)
        {
            var sleep = timeout ?? 0;

            var result = await _httpClient.GetAsync($"http://localhost:7071/api/CircuitBreakerApi?timeout={sleep}");

            if (result.IsSuccessStatusCode)
                using (var responseStream = await result.Content.ReadAsStreamAsync())
                {
                    var elapsed = await JsonSerializer.DeserializeAsync<double>(responseStream);

                    return new OkObjectResult(
                        BuildCircuitBreakerResult($"Executed in {Math.Round(elapsed, 2)} milliseconds."));
                }

            return new BadRequestResult();
        }

        private IActionResult ExecuteCircuitBreakerFallback()
        {
            return new OkObjectResult(
                BuildCircuitBreakerResult("Service is temporarily unavailable. Please try again later"));
        }

        private CircuitBreakerResult BuildCircuitBreakerResult(string message)
        {
            return new CircuitBreakerResult
            {
                Message = message,
                CircuitBreakerStatus = _circuitBreaker.CircuitState.ToString(),
                ExceptionMessage = _circuitBreaker.LastException != null ? _circuitBreaker.LastException.Message : ""
            };
        }
    }
}