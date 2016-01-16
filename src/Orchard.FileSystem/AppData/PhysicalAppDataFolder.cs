using Microsoft.AspNet.FileProviders;
using Microsoft.Extensions.Logging;
using System.IO;

namespace Orchard.FileSystem.AppData
{
    public class PhysicalAppDataFolder : OrchardFileSystem, IAppDataFolder
    {
        public PhysicalAppDataFolder(IAppDataFolderRoot root,
            ILogger<PhysicalAppDataFolder> logger): base(
                root.RootFolder,
                new PhysicalFileProvider(root.RootFolder),
                logger
                )
        {
            if (!Directory.Exists(root.RootFolder))
                Directory.CreateDirectory(root.RootFolder);
        }
    }
}