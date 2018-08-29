using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Spi;
using System;

namespace Dwragge.BlobBlaze.Web
{
    public class DotnetCoreJobFactory : IJobFactory
    {
        private IServiceProvider _services;
        private ILogger _logger;

        public DotnetCoreJobFactory(IServiceProvider services, ILogger<DotnetCoreJobFactory> logger)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _logger = logger ?? throw new ArgumentException(nameof(logger));
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            try
            {
                var detail = bundle.JobDetail;
                var jobType = detail.JobType;

                return (IJob)_services.GetService(jobType);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to instantiate class: {e.Message}");
                var schedException = new SchedulerException($"Failed to instantiate class.", e);
                throw schedException;
            }
        }

        public void ReturnJob(IJob job)
        {
            throw new NotImplementedException();
        }
    }
}
