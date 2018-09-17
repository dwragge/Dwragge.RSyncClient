using Dwragge.BlobBlaze.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dwragge.BlobBlaze.Entities;
using FluentValidation;

namespace Dwragge.BlobBlaze.Web.Controllers
{
    [Route("api/remotes/{remoteId}/[controller]")]
    public class BackupFoldersController : Controller
    {
        private readonly IApplicationContextFactory _contextFactory;

        public BackupFoldersController(IApplicationContextFactory contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        [HttpGet("")]
        public async Task<IActionResult> GetBackupFolders(int remoteId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                var folders = await context.BackupFolders.AsNoTracking().Where(f => f.BackupRemoteId == remoteId).ToListAsync();
                return Ok(folders);
            }
        }

        [HttpPost("")]
        public async Task<IActionResult> AddNewFolder(int remoteId, [FromBody] AddFolderFormData data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (var context = _contextFactory.CreateContext())
            {
                var remote = await context.BackupRemotes.FindAsync(remoteId);
                var backupFolder = new BackupFolder(data.Path, remote)
                {
                    RemoteBaseFolder = data.RemoteFolder,
                    SyncTime = TimeValue.Parse(data.SyncTime),
                    SyncTimeSpan = TimeSpan.Parse(data.SyncTimeSpan)
                };

                await context.BackupFolders.AddAsync(backupFolder);
                await context.SaveChangesAsync();

                return Created(Request.Path.ToString() + "/" + backupFolder.BackupFolderId, remote);
            }
        }
    }

    public class AddFolderFormData
    {
        [Required]
        public string SyncTime { get; set; }
        [Required]
        public string Path { get; set; }
        public string RemoteFolder { get; set; }
        [Required]
        public string SyncTimeSpan { get; set; }
    }

    public class AddFolderFormDataValidator : AbstractValidator<AddFolderFormData>
    {
        public AddFolderFormDataValidator()
        {
            RuleFor(x => x.Path).Must(BeValidDirectory).WithMessage("Path must be a directory that exists on disk.");
            RuleFor(x => x.SyncTime).Must(BeValidTime).WithMessage("Must be a valid time");
            RuleFor(x => x.RemoteFolder).Must(BeValidPath).WithMessage("Remote Folder must be a valid path string");
            RuleFor(x => x.SyncTimeSpan).Must(BeValidTimeSpan).WithMessage("Invalid TimeSpan");
        }

        private bool BeValidDirectory(string path)
        {
            return Directory.Exists(path);
        }

        private bool BeValidTime(string time)
        {
            return TimeValue.TryParse(time, out var _);
        }

        private bool BeValidTimeSpan(string timeSpan)
        {
            return TimeSpan.TryParse(timeSpan, out var _);
        }

        private bool BeValidPath(string path)
        {
            return !Path.GetInvalidFileNameChars().Any(path.Contains);
        }
    }
}
