using Microsoft.AspNet.FileProviders;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Microsoft.Extensions.Logging;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Orchard.FileSystem
{
    public interface IOrchardFileSystem
    {
        string RootPath { get; }

        IFileInfo GetFileInfo(string path);
        IFileInfo GetDirectoryInfo(string path);

        IEnumerable<IFileInfo> ListFiles(string path, Matcher matcher);

        string Combine(params string[] paths);

        /// <summary>
        /// Creates or overwrites the file in the specified path with the specified content.
        /// </summary>
        /// <param name="path">The path and name of the file to create.</param>
        /// <param name="content">The content to write in the created file.</param>
        /// <remarks>If the folder doesn't exist, it will be created.</remarks>
        void CreateFile(string path, string content);

        /// <summary>
        /// Creates or overwrites the file in the specified path.
        /// </summary>
        /// <param name="path">The path and name of the file to create.</param>
        /// <returns>
        /// A <see cref="Stream"/> that provides read/write access to the file specified in path.
        /// </returns>
        /// <remarks>If the folder doesn't exist, it will be created.</remarks>
        Stream CreateFile(string path);

        /// <summary>
        /// Opens a text file, reads all lines of the file, and then closes the file.
        /// </summary>
        /// <param name="path">The path and name of the file to read.</param>
        /// <returns>A string containing all lines of the file, or <code>null</code> if the file doesn't exist.</returns>
        string ReadFile(string path);

        /// <summary>
        /// Open an existing file for reading.
        /// </summary>
        /// <param name="path">The path and name of the file to create.</param>
        /// <returns>
        /// A <see cref="Stream"/> that provides read access to the file specified in path.
        /// </returns>
        Stream OpenFile(string path);
        void StoreFile(string sourceFileName, string destinationPath);
        void DeleteFile(string path);

        DateTime GetFileLastWriteTimeUtc(string path);

        void CreateDirectory(string path);
        bool DirectoryExists(string path);
    }

    public abstract class OrchardFileSystem : IOrchardFileSystem
    {
        private readonly IFileProvider _fileProvider;
        private readonly ILogger _logger;

        public OrchardFileSystem(string rootPath,
            IFileProvider fileProvider,
            ILogger logger)
        {
            _fileProvider = fileProvider;
            _logger = logger;

            RootPath = rootPath;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public string RootPath
        {
            get; private set;
        }

        private void MakeDestinationFileNameAvailable(string destinationFileName)
        {
            var directory = GetDirectoryInfo(destinationFileName);
            // Try deleting the destination first
            try
            {
                if (directory.Exists)
                {
                    if (directory.IsDirectory)
                    {
                        Directory.Delete(destinationFileName);
                    }
                    else
                    {
                        File.Delete(destinationFileName);
                    }
                }
            }
            catch
            {
                // We land here if the file is in use, for example. Let's move on.
            }

            if (directory.IsDirectory && GetDirectoryInfo(destinationFileName).Exists)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogWarning("Could not delete recipe execution folder {0} under \"App_Data\" folder", destinationFileName);
                }
                return;
            }
            // If destination doesn't exist, we are good
            if (!GetFileInfo(destinationFileName).Exists)
                return;

            // Try renaming destination to a unique filename
            const string extension = "deleted";
            for (int i = 0; i < 100; i++)
            {
                var newExtension = (i == 0 ? extension : string.Format("{0}{1}", extension, i));
                var newFileName = Path.ChangeExtension(destinationFileName, newExtension);
                try
                {
                    File.Delete(newFileName);
                    File.Move(destinationFileName, newFileName);

                    // If successful, we are done...
                    return;
                }
                catch
                {
                    // We need to try with another extension
                }
            }

            // Try again with the original filename. This should throw the same exception
            // we got at the very beginning.
            try
            {
                File.Delete(destinationFileName);
            }
            catch (Exception ex)
            {
                if (ex.IsFatal())
                {
                    throw;
                }
                throw new OrchardCoreException(T("Unable to make room for file \"{0}\" in \"App_Data\" folder", destinationFileName), ex);
            }
        }

        /// <summary>
        /// Combine a set of paths in to a signle path
        /// </summary>
        public string Combine(params string[] paths)
        {
            return Path.Combine(paths).Replace(RootPath, string.Empty).Replace(Path.DirectorySeparatorChar, '/').TrimStart('/');
        }

        public void CreateFile(string path, string content)
        {
            using (var stream = CreateFile(path))
            {
                using (var tw = new StreamWriter(stream))
                {
                    tw.Write(content);
                }
            }
        }

        public Stream CreateFile(string path)
        {
            var fileInfo = _fileProvider.GetFileInfo(path);
            if (!fileInfo.Exists)
                Directory.CreateDirectory(Path.GetDirectoryName(fileInfo.PhysicalPath));
            return File.Create(fileInfo.PhysicalPath);
        }

        public string ReadFile(string path)
        {
            var file = _fileProvider.GetFileInfo(path);
            return file.Exists ? File.ReadAllText(file.PhysicalPath) : null;
        }

        public Stream OpenFile(string path)
        {
            return _fileProvider.GetFileInfo(path).CreateReadStream();
        }

        public void StoreFile(string sourceFileName, string destinationPath)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Storing file \"{0}\" as \"{1}\" in \"App_Data\" folder", sourceFileName, destinationPath);
            }

            var destinationFileName = GetFileInfo(destinationPath).PhysicalPath;
            MakeDestinationFileNameAvailable(destinationFileName);
            File.Copy(sourceFileName, destinationFileName, true);
        }

        public void DeleteFile(string path)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Deleting file \"{0}\" from \"App_Data\" folder", path);
            }

            MakeDestinationFileNameAvailable(GetFileInfo(path).PhysicalPath);
        }

        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(GetFileInfo(path).PhysicalPath);
        }

        public bool DirectoryExists(string path)
        {
            return GetFileInfo(path).Exists;
        }

        public DateTime GetFileLastWriteTimeUtc(string path)
        {
            return File.GetLastWriteTimeUtc(GetFileInfo(path).PhysicalPath);
        }

        public IFileInfo GetFileInfo(string path)
        {
            return _fileProvider.GetFileInfo(path);
        }

        public IFileInfo GetDirectoryInfo(string path)
        {
            return _fileProvider.GetFileInfo(path);
        }
        
        public IEnumerable<IFileInfo> ListFiles(string path, Matcher matcher)
        {
            var directory = GetDirectoryInfo(CleanPath(path));
            if (!directory.Exists) {
                return Enumerable.Empty<IFileInfo>();
            }
            
            return matcher.Execute(
                new DirectoryInfoWrapper(
                    new DirectoryInfo(directory.PhysicalPath)))
                    .Files
                    .Select(result => GetFileInfo(result.Path));
        }

        private string CleanPath(string path) {
            return path.TrimStart('~', '/');
        }
    }
}
