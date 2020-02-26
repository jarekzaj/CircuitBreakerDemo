namespace CircuitBreaker
{
    public class CircuitBreakerResult
    {
        public string Message { get; set; }
        public string CircuitBreakerStatus { get; set; }
        public string ExceptionMessage { get; set; }
    }
}