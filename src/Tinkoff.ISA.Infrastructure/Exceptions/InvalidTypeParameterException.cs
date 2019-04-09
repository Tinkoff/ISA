using System;

namespace Tinkoff.ISA.Infrastructure.Exceptions
{
    public class InvalidTypeParameterException : Exception
    {
        public InvalidTypeParameterException(Type type, Type expectedType)
        {
            Message =
                $"Invalid type parameter for type '{type.FullName}':" +
                $" expected type '{expectedType.FullName}'";
        }

        public InvalidTypeParameterException(Type type, Type expectedType, Type receivedType)
        {
            Message =
                $"Invalid type parameter for type '{type.FullName}':" +
                $" expected type '{expectedType.FullName}', but received '{receivedType.FullName}'";
        }

        public override string Message { get; }
    }
}
