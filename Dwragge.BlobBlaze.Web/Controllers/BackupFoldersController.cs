using Dwragge.BlobBlaze.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dwragge.BlobBlaze.Application.Notifications;
using Dwragge.BlobBlaze.Application.Requests;
using Dwragge.BlobBlaze.Entities;
using FluentValidation;
using MediatR;

namespace Dwragge.BlobBlaze.Web.Controllers
{
    [Route("api/remotes/{remoteId}/[controller]")]
    public class BackupFoldersController : Controller
    {
        private readonly IApplicationContextFactory _contextFactory;
        private readonly IMediator _mediator;

        public BackupFoldersController(IApplicationContextFactory contextFactory, IMediator mediator)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _mediator = mediator;
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
                if (remote == null) return NotFound();

                var backupFolder = new BackupFolder(data.Path, remote)
                {
                    Name = data.Name,
                    RemoteBaseFolder = data.RemoteFolder,
                    SyncTime = TimeValue.Parse(data.SyncTime),
                    SyncTimeSpan = TimeSpan.Parse(data.SyncTimeSpan)
                };

                await context.BackupFolders.AddAsync(backupFolder);
                await context.SaveChangesAsync();

                await _mediator.Publish(new FolderCreatedNotification(backupFolder));

                return Created(Request.Path.ToString() + "/" + backupFolder.BackupFolderId, backupFolder);
            }
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> EditBackupFolder(int remoteId, int id, [FromBody] AddFolderFormData data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (var context = _contextFactory.CreateContext())
            {
                var folder = await context.BackupFolders.FindAsync(id);
                if (folder == null) return NotFound();
                if (folder.BackupRemoteId != remoteId) return NotFound();

                folder.Name = data.Name;
                folder.RemoteBaseFolder = data.RemoteFolder;
                folder.SyncTime = TimeValue.Parse(data.SyncTime);
                folder.SyncTimeSpan = TimeSpan.Parse(data.SyncTimeSpan);

                await context.SaveChangesAsync();
                await _mediator.Publish(new FolderChangedNotification(folder));

                return Created(Request.Path.ToString() + "/" + folder.BackupFolderId, folder);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBackupFolder(int remoteId, int id)
        {
            using (var context = _contextFactory.CreateContext())
            {
                var folder = await context.BackupFolders.FindAsync(id);
                if (folder == null) return NotFound();
                if (folder.BackupRemoteId != remoteId) return NotFound();

                context.BackupFolders.Remove(folder);
                await context.SaveChangesAsync();
            }

            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBackupFolder(int remoteId, int id)
        {
            using (var context = _contextFactory.CreateContext())
            {
                var backupFolder = await context.BackupFolders.FindAsync(id);
                if (backupFolder == null) return NotFound();
                if (backupFolder.BackupRemoteId != remoteId) return NotFound();

                var jsonObj = new
                {
                    backupFolder.Path,
                    backupFolder.Name,
                    RemoteFolder = backupFolder.RemoteBaseFolder,
                    SyncDays = backupFolder.SyncTimeSpan.Days,
                    SyncHours = backupFolder.SyncTimeSpan.Hours,
                    SyncMins = backupFolder.SyncTimeSpan.Minutes,
                    Time = backupFolder.SyncTime.ToString()
                };

                return Ok(jsonObj);
            }
        }

        [HttpGet("{id}/sync")]
        public async Task<IActionResult> SyncNow(int remoteId, int id)
        {
            using (var context = _contextFactory.CreateContext())
            {
                var folder = await context.BackupFolders
                    .AsNoTracking()
                    .Include(f => f.Remote)
                    .SingleOrDefaultAsync(f => f.BackupFolderId == id);

                if (folder == null) return NotFound();
                if (folder.BackupRemoteId != remoteId) return NotFound();

                await _mediator.Send(new SyncFolderNowRequest(folder));

                return Ok();
            }
        }
    }

    public class AddFolderFormData
    {
        [Required]
        public string SyncTime { get; set; }
        [Required]
        public string Name { get; set; }
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
            RuleFor(x => x.Name).Length(1, 50);
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
