using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace NuGetFeed.Controllers
{
    [Route("[controller]")]
    public class VersionsController : Controller
    {
        const int CacheMinutes = 15;
        const int MaxVersions = 100;
        private IMemoryCache Cache;

        public VersionsController(IMemoryCache memoryCache)
        {
            Cache = memoryCache;
        }

        [HttpGet]
        public ActionResult Get()
        {
            return Redirect("https://github.com/mganss/NuGetFeed");
        }

        // GET versions/Newtonsoft.Json?prerelase=true&unlisted=true
        [HttpGet("{packageId}")]
        public async Task<ActionResult> Get(string packageId, bool prerelease = false, bool unlisted = false)
        {
            if (!Cache.TryGetValue(packageId, out string content))
            {
                var repo = Repository.Factory.GetCoreV3(NuGetConstants.V3FeedUrl);
                var resource = await repo.GetResourceAsync<MetadataResource>();
                var versions = (await resource.GetVersions(packageId, prerelease, unlisted, new SourceCacheContext(), NullLogger.Instance, CancellationToken.None))
                    .Reverse().Take(MaxVersions);
                XNamespace atom = "http://www.w3.org/2005/Atom";
                var rss = new XElement("rss", new XAttribute("version", "2.0"),
                    new XElement("channel",
                        new XElement("title", packageId),
                        new XElement("description", $"{packageId} NuGet Package Version Feed"),
                        new XElement("link", $"https://www.nuget.org/packages/{packageId}"),
                        new XElement(atom + "link",
                            new XAttribute("rel", "self"),
                            new XAttribute("type", "application/rss+xml"),
                            new XAttribute("href", HttpContext.Request.GetDisplayUrl())
                        )
                    )
                );

                foreach (var version in versions)
                {
                    var link = $"https://www.nuget.org/packages/{packageId}/{version.ToFullString()}";
                    var item = new XElement("item",
                        new XElement("title", $"{packageId} {version.ToFullString()}"),
                        new XElement("guid", link),
                        new XElement("link", link)
                    );

                    rss.Element("channel").Add(item);
                }

                content = rss.ToString();
                Cache.Set(packageId, content, TimeSpan.FromMinutes(CacheMinutes));
            }

            return Content(content, "application/rss+xml");
        }
    }
}
