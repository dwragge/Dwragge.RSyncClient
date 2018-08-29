using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dwragge.RCloneClient.Persistence
{
    public class JobContext : DbContext, IJobContext
    {
        private readonly ILoggerFactory _loggerFactory;

        public JobContext(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public DbSet<BackupFolderDto> BackupFolders { get; set; }
        public DbSet<TrackedFileDto> TrackedFiles { get; set; }
        public DbSet<FileVersionHistoryDto> FileVersionHistory { get; set; }
        public DbSet<RemoteDto> Remotes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var baseFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "rcloneservice");
            var dbPath =  Path.Combine(baseFolder, "service.db");
            var connectionString = $"Data Source={dbPath}";
            Directory.CreateDirectory(baseFolder);
            options.UseSqlite(connectionString);
            options.UseLoggerFactory(_loggerFactory);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TrackedFileDto>()
                .HasIndex(d => d.FileName)
                .IsUnique();
        }
    }
}
