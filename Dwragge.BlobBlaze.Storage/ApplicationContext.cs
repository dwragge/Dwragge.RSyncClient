using Dwragge.BlobBlaze.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace Dwragge.BlobBlaze.Storage
{
    public class ApplicationContext : DbContext
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IConfiguration _configuration;

        public ApplicationContext(ILoggerFactory factory, IConfiguration configuration)
        {
            _loggerFactory = factory;
            _configuration = configuration;
        }

        public DbSet<BackupFolder> BackupFolders { get; set; }
        public DbSet<BackupRemote> BackupRemotes { get; set; }
        public DbSet<TrackedFile> TrackedFiles { get; set; }
        public DbSet<TrackedFileVersion> TrackedFileVersions { get; set; }
        public DbSet<BackupFolderJob> BackupJobs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var baseFolder = Path.Combine(appDataFolder, "BlobBlaze");
            var dbPath = Path.Combine(baseFolder, "service.db");
            var connectionString = "Data Source=" + dbPath;

            Directory.CreateDirectory(baseFolder);
            options.UseSqlite(connectionString);
            
            if (_configuration.GetValue<bool>("Db:LogGeneratedSql") == true)
            {
                options.UseLoggerFactory(_loggerFactory);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TrackedFile>()
                .HasIndex(d => d.FileName)
                .IsUnique();
        }
    }
}
