using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Threading;

namespace CircuitBreakerApi
{
    public static class CircuitBreakerApi
    {
        [FunctionName("CircuitBreakerApi")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            var stopwatch = Stopwatch.StartNew();
            log.LogInformation("C# HTTP trigger function processed a request.");

            string timeout = req.Query["timeout"];

            if (timeout != null) Thread.Sleep(Convert.ToInt32(timeout));

            stopwatch.Stop();

            return new OkObjectResult(stopwatch.Elapsed.TotalMilliseconds);
        }
    }
}
