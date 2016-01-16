using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Logging;
using Orchard.FileSystem;

namespace Orchard.Hosting.FileSystems
{
    public class WebFileSystem : OrchardFileSystem
    {
        public WebFileSystem(IHostingEnvironment hostingEnvironment,
            ILogger<WebFileSystem> logger)
            : base(
                  hostingEnvironment.WebRootPath,
                  hostingEnvironment.WebRootFileProvider, 
                  logger)
        {
        }
    }
}
