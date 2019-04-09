using System.Collections.Generic;

namespace Tinkoff.ISA.AppLayer.Slack.Common
{
    public interface ICallbackIdCustomParamsWrappingService
    {
        IList<string> Unwrap(string callBackId);

        string Wrap(string callBackId, IEnumerable<string> customParams);
    }
}
