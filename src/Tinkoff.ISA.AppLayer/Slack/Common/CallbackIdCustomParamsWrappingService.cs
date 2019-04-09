using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tinkoff.ISA.AppLayer.Slack.Common
{
    internal class CallbackIdCustomParamsWrappingService : ICallbackIdCustomParamsWrappingService
    {
        private const string Delimiter = ":";
        private const int MaxCallbackIdLength = 200;

        public IList<string> Unwrap(string callBackId) => callBackId
            .Split(Delimiter, StringSplitOptions.RemoveEmptyEntries).Skip(1).ToList();

        public string Wrap(string callBackId, IEnumerable<string> customParams)
        {
            if (callBackId == null) throw new ArgumentNullException(nameof(callBackId));
            if (customParams == null) throw new ArgumentNullException(nameof(customParams));

            var builder = new StringBuilder(callBackId + Delimiter);

            foreach (var customParam in customParams)
            {
                builder.Append(customParam).Append(Delimiter);
            }

            if (builder.Length > MaxCallbackIdLength)
                throw new ArgumentException($"The length of callbackId exceeds maximum {MaxCallbackIdLength}");

            return builder.ToString();
        }
    }
}
