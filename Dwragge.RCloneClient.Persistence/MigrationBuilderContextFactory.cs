using Microsoft.EntityFrameworkCore.Design;

namespace Dwragge.RCloneClient.Persistence
{
    public class MigrationBuilderContextFactory : IDesignTimeDbContextFactory<JobContext>
    {
        public JobContext CreateDbContext(string[] args)
        {
            return new JobContext(null);
        }
    }
}
