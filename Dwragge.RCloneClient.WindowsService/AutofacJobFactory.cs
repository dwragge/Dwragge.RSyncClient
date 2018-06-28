using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;
using NLog;
using Quartz;
using Quartz.Spi;

namespace Dwragge.RCloneClient.WindowsService
{
    public class AutofacJobFactory : IJobFactory
    {
        private readonly IContainer _container;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public AutofacJobFactory(IContainer container)
        {
            _container = container;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            try
            {
                var detail = bundle.JobDetail;
                var jobType = detail.JobType;

                return (IJob) _container.Resolve(jobType);
            }
            catch (Exception e)
            {
                _logger.Error($"Failed to instantiate class: {e.Message}");
                var schedException = new SchedulerException($"Failed to instantiate class.", e);
                throw schedException;
            }
        }

        public void ReturnJob(IJob job)
        {
            var disposable = job as IDisposable;
            disposable?.Dispose();
        }
    }
}