using Dwragge.BlobBlaze.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dwragge.BlobBlaze.Web.Controllers
{
    [Route("api/[controller]")]
    public class BackupFoldersController : Controller
    {
        private readonly IApplicationContextFactory _contextFactory;

        public BackupFoldersController(IApplicationContextFactory contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        [HttpGet("{remoteId}")]
        public async Task<IActionResult> GetBackupFolders(int remoteId)
        {
            using (var context = _contextFactory.CreateContext())
            {
                var folders = await context.BackupFolders.Where(f => f.BackupRemoteId == remoteId).ToListAsync();
                return Ok(folders);
            }
        }
    }
}
