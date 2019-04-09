using System;
using System.Threading.Tasks;

namespace Tinkoff.ISA.AppLayer.Slack.Executing
{
    public interface ISlackExecutorService
    {
        Task ExecuteAction(Type paramsType, params object[] args);

        Task ExecuteSubmission(Type paramsType, params object[] args);
    }
}
