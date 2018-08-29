using Microsoft.EntityFrameworkCore.Design;


namespace Dwragge.BlobBlaze.Storage
{
    public class DesignTimeContextFactory : IDesignTimeDbContextFactory<ApplicationContext>
    {
        public ApplicationContext CreateDbContext(string[] args)
        {
            return new ApplicationContext(null, null, null);
        }
    }
}
