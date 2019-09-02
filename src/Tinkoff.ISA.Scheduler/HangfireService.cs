using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Hosting;

namespace Tinkoff.ISA.Scheduler
{
    public class HangfireService : IHostedService
    {
        private readonly IEnumerable<IBackgroundProcess> _additionalProcesses;
        private readonly BackgroundJobServerOptions _options;
        private readonly JobStorage _storage;
        private BackgroundJobServer _server;

        public HangfireService(
            JobStorage storage,
            IEnumerable<BackgroundJobServerOptions> options,
            IEnumerable<IBackgroundProcess> additionalProcesses)
        {
            _storage = storage;
            _options = options.FirstOrDefault() ?? new BackgroundJobServerOptions();
            _additionalProcesses = additionalProcesses;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _server = new BackgroundJobServer(_options, _storage, _additionalProcesses);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _server.Dispose();
            return Task.CompletedTask;
        }
    }
}