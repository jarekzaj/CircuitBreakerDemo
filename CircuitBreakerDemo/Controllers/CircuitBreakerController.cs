using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace CircuitBreaker.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CircuitBreakerController : ControllerBase
    {
        private readonly ICircuitBreakerClient _circuitBreakerClient;

        public CircuitBreakerController(ICircuitBreakerClient circuitBreakerClient)
        {
            _circuitBreakerClient = circuitBreakerClient;
        }

        [HttpGet]
        public Task<IActionResult> Get(int? timeout)
        {
            return _circuitBreakerClient.ExecuteCircuitBreakerPolicy(timeout);
        }
    }
}