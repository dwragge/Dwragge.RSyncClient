using Dwragge.BlobBlaze.Entities;
using Dwragge.BlobBlaze.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dwragge.BlobBlaze.Web.Controllers
{
    [Route("api/[controller]")]
    public class RemotesController : Controller
    {
        private IApplicationContextFactory _contextFactory;

        public RemotesController(IApplicationContextFactory contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        [HttpGet("")]
        public IActionResult GetRemotes()
        {
            using (var context = _contextFactory.CreateContext())
            {
                var remotes = context.BackupRemotes.ToList();
                return Ok(remotes);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRemote(int id)
        {
            using (var context = _contextFactory.CreateContext())
            {
                var remote = await context.BackupRemotes.FindAsync(id);
                if (remote == null) return NotFound();

                if (remote.Default)
                {
                    var next = await context.BackupRemotes.FirstOrDefaultAsync();
                    if (next != null) next.Default = true;
                }

                context.BackupRemotes.Remove(remote);
                await context.SaveChangesAsync();
            }

            return Ok();
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> UpdateRemote(int id, [FromBody] AddNewRemoteFormData data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!AzureConnectionString.TryParse(data.ConnectionString, out var connectionStirngObj))
            {
                ModelState.AddModelError(nameof(data.ConnectionString), "Connection String Was Invalid");
                return BadRequest(ModelState);
            }

            using (var context = _contextFactory.CreateContext())
            {
                var remote = await context.BackupRemotes.FindAsync(id);
                if (remote == null) return NotFound();

                remote.BaseFolder = data.BaseFolder;
                remote.ConnectionString = connectionStirngObj;
                remote.Default = data.Default;
                remote.Name = data.Name;

                if (data.Default)
                {
                    var currentDefault = await context.BackupRemotes.SingleOrDefaultAsync(r => r.Default);
                    if (currentDefault != null) currentDefault.Default = false;
                }

                await context.SaveChangesAsync();

                return Created(Request.Path.ToString() + "/" + remote.BackupRemoteId, remote);
            }
        }

        [HttpPost("")]
        public async Task<IActionResult> AddNewRemote([FromBody]AddNewRemoteFormData data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!AzureConnectionString.TryParse(data.ConnectionString, out var connectionStringObj))
            {
                ModelState.AddModelError(nameof(data.ConnectionString), "Connection String Was Invalid");
                return BadRequest(ModelState);
            }

            var remote = new BackupRemote(data.Name, data.BaseFolder, connectionStringObj)
            {
                Default = data.Default
            };

            using (var context = _contextFactory.CreateContext())
            {
                if (data.Default)
                {
                    var currentDefault = await context.BackupRemotes.SingleOrDefaultAsync(r => r.Default == true);
                    if (currentDefault != null) currentDefault.Default = false;
                }

                await context.BackupRemotes.AddAsync(remote);
                await context.SaveChangesAsync();
            }

            return Created(Request.Path.ToString() + "/" + remote.BackupRemoteId, remote);
        }
    }
    
    public class AddNewRemoteFormData
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        public string ConnectionString { get; set; }
    
        public bool Default { get; set; }

        public string BaseFolder { get; set; }
    }
}