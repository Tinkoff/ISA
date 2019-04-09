using System;

namespace Tinkoff.ISA.AppLayer.Slack.TypeChooser.ActionParams
{
    public interface ISlackParamsSelectService
    {
        Type Choose(string actionName);
    }
}
