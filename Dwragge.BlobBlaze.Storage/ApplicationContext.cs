using Dwragge.BlobBlaze.Entities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Dwragge.BlobBlaze.Storage
{
    public class ApplicationContext : DbContext
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly IConfiguration _configuration;
        private readonly IDataProtectionProvider _protectionProvider;
        private IDataProtector _protector;

        public ApplicationContext(ILoggerFactory factory, IConfiguration configuration, IDataProtectionProvider protectionProvider)
        {
            _loggerFactory = factory;
            _configuration = configuration;
            _protectionProvider = protectionProvider;
        }
        

        public DbSet<BackupFolder> BackupFolders { get; set; }
        public DbSet<BackupRemote> BackupRemotes { get; set; }
        public DbSet<TrackedFile> TrackedFiles { get; set; }
        public DbSet<TrackedFileVersion> TrackedFileVersions { get; set; }
        public DbSet<BackupFolderJob> BackupJobs { get; set; }
        public DbSet<BackupFileUploadJob> UploadJobs { get; set; }
        public DbSet<UploadError> UploadErrors { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var baseFolder = Path.Combine(appDataFolder, "BlobBlaze");
            var dbPath = Path.Combine(baseFolder, "service.db");
            var connectionString = "Data Source=" + dbPath;

            Directory.CreateDirectory(baseFolder);
            options.UseSqlite(connectionString);
            
            if (_configuration?.GetValue<bool>("Db:LogGeneratedSql") == true)
            {
                options.UseLoggerFactory(_loggerFactory);
            }
        }

        private string EncryptString(AzureConnectionString connectionString)
        {
            if (_protector == null)
            {
                _protector = _protectionProvider.CreateProtector("BlobBlaze.ApplicationContext");
            }

            var bytes = _protector.Protect(Encoding.UTF8.GetBytes(connectionString.ToString().ToCharArray()));
            return Convert.ToBase64String(bytes);
        }

        private AzureConnectionString DecryptString(string encrypted)
        {
            if (_protector == null)
            {
                _protector = _protectionProvider.CreateProtector("BlobBlaze.ApplicationContext");
            }

            byte[] bytes = _protector.Unprotect(Convert.FromBase64String(encrypted));
            var rawString = Encoding.UTF8.GetString(bytes);
            if (!AzureConnectionString.TryParse(rawString, out var connectionString))
            {
                throw new InvalidOperationException("Decrypted String is invalid");
            }

            return connectionString;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TrackedFile>()
                .HasIndex(d => d.FileName)
                .IsUnique();

            modelBuilder.Entity<BackupFolder>()
                .Property(nameof(BackupFolder.Size))
                .HasDefaultValue(-1);
            modelBuilder.Entity<BackupFolder>()
                .HasMany<BackupFolderJob>()
                .WithOne(x => x.Folder);
            modelBuilder.Entity<BackupFolder>()
                .Property(t => t.SyncTime)
                .HasConversion(to => to.ToString(), from => TimeValue.Parse(from));

            modelBuilder.Entity<BackupFolderJob>()
                .Property(j => j.Status)
                .HasConversion(new EnumToStringConverter<BackupFolderJobStatus>());

            modelBuilder.Entity<BackupFileUploadJob>()
                .Property(x => x.LocalFile)
                .HasConversion(fileInfo => fileInfo.FullName, path => new FileInfo(path));
            modelBuilder.Entity<BackupFileUploadJob>()
                .Property(j => j.Status)
                .HasConversion(new EnumToStringConverter<BackupFileUploadJobStatus>());

            modelBuilder.Entity<BackupRemote>()
                .Property(t => t.ConnectionString)
                .HasConversion(unencrypted => EncryptString(unencrypted), encrypted => DecryptString(encrypted));
        }
    }
}
