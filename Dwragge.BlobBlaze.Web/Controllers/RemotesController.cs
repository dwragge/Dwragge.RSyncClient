using Microsoft.AspNetCore.Mvc;

namespace Dwragge.BlobBlaze.Web.Controllers
{
    [Route("api/[controller]")]
    public class RemotesController : Controller
    {
        [HttpGet("")]
        public IActionResult GetRemotes()
        {
            var remotes = new[]
            {
                new Remote()
                {
                    Id = 1,
                    Name = "Development",
                    UrlName = "development",
                    Default = true
                },
                new Remote()
                {
                    Id = 2,
                    Name = "Azure",
                    UrlName = "azure",
                    Default = false
                },
                new Remote()
                {
                    Id = 3,
                    Name = "Azure 2",
                    UrlName = "azure-2",
                    Default = false
                }
            };

            return Ok(remotes);
        }
    }

    public class Remote
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string UrlName { get; set; }
        public bool Default {get; set; }
    }
}