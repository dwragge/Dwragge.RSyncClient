using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Web;

namespace Dwragge.BlobBlaze.Web.Controllers
{
    [Route("api/[controller]")]
    public class FileSystemController : Controller
    {
        [HttpGet("query/{*query}")]
        public IActionResult DiscoverPaths(string query)
        {
            if (string.IsNullOrEmpty(query)) return Ok(Enumerable.Empty<string>());
            query = HttpUtility.UrlDecode(query);
            query = query.Replace('/', '\\');
            // weird edge case where C: returns everything in the current directory???
            if (query.Length == 2)
                return Ok(Enumerable.Empty<string>());
            try
            {
                if (Directory.Exists(query))
                {
                    var info = new DirectoryInfo(query);
                    var directories = info.EnumerateDirectories();
                    var names = directories.Select(x => x.FullName).ToList();
                    names.Insert(0, info.FullName);
                    return Ok(names);
                }
                else
                {
                    var pathUri = new Uri(query);
                    var parent = string.Join("", pathUri.Segments.Take(pathUri.Segments.Count() - 1).Skip(1));
                    var info = new DirectoryInfo(parent);
                    var queryPart = pathUri.Segments.Last();
                    var allDirectories = info.EnumerateDirectories();
                    var matchingDirectories = allDirectories.Where(x => x.Name.StartsWith(queryPart, StringComparison.OrdinalIgnoreCase));
                    return Ok(matchingDirectories.Select(x => x.FullName));
                }
            }
            catch (Exception)
            {
                return Ok(Enumerable.Empty<string>());
            }
        }
    }
}
