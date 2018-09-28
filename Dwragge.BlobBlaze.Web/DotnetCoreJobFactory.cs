using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Spi;
using System;

namespace Dwragge.BlobBlaze.Web
{
    public class DotnetCoreJobFactory : IJobFactory
    {
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;

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

                _logger.LogInformation("Resolving {type} for job {key}", jobType, detail.Key);

                var job = (IJob)_services.GetService(jobType);
                if (job == null) throw new InvalidOperationException($"Couldn't instantiate class of type {jobType}, services.GetService() returned null");
                return job;
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
        }
    }
}
