namespace Dwragge.RCloneClient.Persistence
{
    public interface IJobContextFactory
    {
        JobContext CreateContext(bool shouldLog = true);
    }
}