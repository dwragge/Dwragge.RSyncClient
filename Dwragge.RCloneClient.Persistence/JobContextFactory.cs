using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Dwragge.RCloneClient.Persistence
{
    public class JobContextFactory : IJobContextFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        public JobContextFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public JobContext CreateContext(bool shouldLog = true)
        {
            if (shouldLog) return new JobContext(_loggerFactory);
            else return new JobContext(null);
        }
    }
}
