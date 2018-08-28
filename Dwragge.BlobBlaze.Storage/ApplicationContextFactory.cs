using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Dwragge.BlobBlaze.Storage
{
    public class ApplicationContextFactory : IApplicationContextFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IConfiguration _configuration;

        public ApplicationContextFactory(ILoggerFactory factory, IConfiguration configuration)
        {
            _loggerFactory = factory;
            _configuration = configuration;
        }

        public ApplicationContext CreateContext()
        {
            return new ApplicationContext(_loggerFactory, _configuration);
        }
    }
}
