using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Tinkoff.ISA.AppLayer.Slack.Dialogs;

namespace Tinkoff.ISA.AppLayer.Slack.Executing
{
    internal class SlackExecutorService : ISlackExecutorService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<Type, MethodInfo[]> _actionHandlerMethods = new Dictionary<Type, MethodInfo[]>();

        public SlackExecutorService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task ExecuteAction(Type paramsType, params object[] args)
        {
            return Execute(paramsType, typeof(ISlackActionHandler<>), args);
        }

        public Task ExecuteSubmission(Type paramsType, params object[] args)
        {
            return Execute(paramsType, typeof(IDialogSubmissionService<>), args);
        }

        private Task Execute(Type paramsType, Type interfaceType, params object[] args)
        {
            if (paramsType == null) throw new ArgumentNullException(nameof(paramsType));
            if (args.Length == 0) throw new ArgumentException(nameof(args));

            var actionHandlerType = interfaceType.MakeGenericType(paramsType);
            var actionHandlerService = _serviceProvider.GetService(actionHandlerType);

            _actionHandlerMethods.TryGetValue(actionHandlerType, out var methods);
            if (methods == null)
            {
                methods = actionHandlerType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                _actionHandlerMethods.Add(actionHandlerType, methods);
            }

            if (methods.Length != 1)
                throw new ArgumentException($"Interface {interfaceType} must contain exactly one method!");

            return (Task)actionHandlerType.InvokeMember(methods.First().Name, BindingFlags.InvokeMethod, null, actionHandlerService, args);
        }
    }
}
