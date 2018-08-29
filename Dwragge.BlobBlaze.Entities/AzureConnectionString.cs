using System;

namespace Dwragge.BlobBlaze.Entities
{
    public class AzureConnectionString
    {
        public string EndpointsProtocol { get; set; } = "https";
        public string AccountName { get; set; }
        public string AccountKey { get; set; }
        public string EndpointSuffix { get; set; }
        public bool IsDevelopment { get; private set; }

        private AzureConnectionString()
        {

        }

        public override string ToString()
        {
            return $"DefaultEndpointsProtocol={EndpointsProtocol};AccountName={AccountName};AccountKey={AccountKey};EndpointSuffix={EndpointSuffix}";
        }

        public static bool TryParse(string connectionString, out AzureConnectionString obj)
        {
            obj = null;
            var splits = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
            if (splits.Length < 2 || splits.Length > 3)
            {
                return false;
            }

            string protocol = "https", account = null, key = null, suffix = "core.windows.net";
            foreach (var split in splits)
            {
                if (split.StartsWith("DefaultEndpointsProtocol=", StringComparison.OrdinalIgnoreCase))
                {
                    protocol = split.Substring(25);
                }
                else if (split.StartsWith("AccountName=", StringComparison.OrdinalIgnoreCase))
                {
                    account = split.Substring(11);
                }
                else if (split.StartsWith("AccountKey=", StringComparison.OrdinalIgnoreCase))
                {
                    key = split.Substring(10);
                }
                else if (split.StartsWith("EndpointSuffix=", StringComparison.OrdinalIgnoreCase))
                {
                    suffix = split.Substring(14);
                }
            }

            if (string.IsNullOrEmpty(account) || string.IsNullOrEmpty(key))
            {
                return false;
            }

            obj = new AzureConnectionString
            {
                EndpointsProtocol = protocol,
                AccountName = account,
                AccountKey = key,
                EndpointSuffix = suffix
            };

            return true;
        }

        public static AzureConnectionString DevelopmentConnection => new AzureConnectionString { IsDevelopment = true };
    }
}
