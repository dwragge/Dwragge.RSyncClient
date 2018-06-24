using System;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace Dwragge.RCloneClient.Persistence
{
    public class JobContext : DbContext
    {   
        public DbSet<BackupFolder> BackupFolders { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dbPath =  Path.Combine(baseFolder, "rcloneservice", "service.db");
            var connectionString = $"Data Source={dbPath}";
            options.UseSqlite(connectionString);
        }
    }
}
