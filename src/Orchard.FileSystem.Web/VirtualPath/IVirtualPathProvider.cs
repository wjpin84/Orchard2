using System;
using System.IO;

namespace Orchard.FileSystem.VirtualPath
{
    public interface IVirtualPathProvider
    {
        string Combine(params string[] paths);
        string MapPath(string virtualPath);

        bool FileExists(string virtualPath);
        Stream OpenFile(string virtualPath);
        string ReadFile(string virtualPath);
        DateTime GetFileLastWriteTimeUtc(string virtualPath);
    }
}