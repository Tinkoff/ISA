using System;

namespace Tinkoff.ISA.DAL.Slack
{
    public class SlackException : Exception
    {
        public SlackException(string message) : base(message)
        {
        }
    }
}