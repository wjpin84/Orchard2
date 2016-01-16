namespace Orchard.FileSystem.AppData
{
    public class AppDataFolderRoot : IAppDataFolderRoot
    {
        private readonly IOrchardFileSystem _fileSystem;
        public AppDataFolderRoot(IOrchardFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public string RootPath => "App_Data";

        public string RootFolder => _fileSystem.MapPath(RootPath);
    }
}