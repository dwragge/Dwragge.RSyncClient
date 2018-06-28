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
        public DbSet<BackedUpFileDto> BackedUpFiles { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dbPath =  Path.Combine(baseFolder, "rcloneservice", "service.db");
            var connectionString = $"Data Source={dbPath}";
            options.UseSqlite(connectionString);
            options.UseLoggerFactory(_loggerFactory);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BackedUpFileDto>()
                .HasIndex(d => d.FileName);

            modelBuilder.Entity<BackedUpFileDto>()
                .HasIndex(d => new {d.FileName, d.RemoteLocation})
                .IsUnique();
        }
    }
}
