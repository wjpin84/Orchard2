using System;
using System.IO;
using Orchard.Environment;
using Microsoft.Extensions.Logging;

namespace Orchard.FileSystem.VirtualPath
{
    public class DefaultVirtualPathProvider : IVirtualPathProvider
    {
        private readonly IHostEnvironment _hostEnvironment;
        private readonly ILogger _logger;

        public DefaultVirtualPathProvider(
            IHostEnvironment hostEnvironment,
            ILogger<DefaultVirtualPathProvider> logger)
        {
            _hostEnvironment = hostEnvironment;
            _logger = logger;
        }

        public virtual string Combine(params string[] paths)
        {
            return Path.Combine(paths).Replace(Path.DirectorySeparatorChar, '/');
        }

        public virtual Stream OpenFile(string virtualPath)
        {
            return File.Open(MapPath(virtualPath), FileMode.Open);
        }

        public virtual string ReadFile(string virtualPath)
        {
            return File.ReadAllText(MapPath(virtualPath));
        }

        public virtual DateTime GetFileLastWriteTimeUtc(string virtualPath)
        {
            return File.GetLastWriteTime(MapPath(virtualPath)).ToUniversalTime();
        }

        public virtual string MapPath(string virtualPath)
        {
            if (!virtualPath.StartsWith("~", StringComparison.OrdinalIgnoreCase))
                return virtualPath;

            return _hostEnvironment.MapPath(virtualPath);
        }

        public virtual bool FileExists(string virtualPath)
        {
            return File.Exists(MapPath(virtualPath));
        }
    }
}