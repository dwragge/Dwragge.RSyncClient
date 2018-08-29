using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Dwragge.BlobBlaze.Storage
{
    public class ApplicationContextFactory : IApplicationContextFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IConfiguration _configuration;
        private readonly IDataProtectionProvider _dataProtectionProvider;

        public ApplicationContextFactory(ILoggerFactory factory, IConfiguration configuration, IDataProtectionProvider provider)
        {
            _loggerFactory = factory;
            _configuration = configuration;
            _dataProtectionProvider = provider;
        }

        public ApplicationContext CreateContext()
        {
            return new ApplicationContext(_loggerFactory, _configuration, _dataProtectionProvider);
        }
    }
}
