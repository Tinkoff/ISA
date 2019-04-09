using System;

namespace Tinkoff.ISA.DAL.Elasticsearch
{
    public class ElasticException : Exception
    {
        public ElasticException(string message) : base(message)
        {
        }
    }
}
