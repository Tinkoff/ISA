using System;

namespace Tinkoff.ISA.Infrastructure.Exceptions
{
    public class ExternalApiInvocationException : Exception
    {
        private readonly string _baseAddress;
        private readonly string _method;
        private readonly string _message;

        public ExternalApiInvocationException(string baseAddress, string method, string message)
        {
            _baseAddress = baseAddress;
            _method = method;
            _message = message;
        }

        public ExternalApiInvocationException(string baseAddress, string method, string message, Exception innerException) : base(message, innerException)
        {
            _baseAddress = baseAddress;
            _method = method;
            _message = message;
        }

        public override string Message =>
            $"Error calling external service method. host: {_baseAddress}. method: {_method}. error: {_message}";
    }
}
