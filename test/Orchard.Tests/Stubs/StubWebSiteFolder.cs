using Microsoft.AspNet.FileProviders;
using Microsoft.Extensions.Primitives;
using Orchard.FileSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Orchard.Tests.Stubs
{
    public class StubWebSiteFolder : OrchardFileSystem
    {
        public StubWebSiteFolder() : base(
                  "Foo",
                  new MockFileProvider(),
                  new NullLogger())
        {
        }

        public StubWebSiteFolder(params IFileInfo[] files) : base(
          "Foo",
          new MockFileProvider(files),
          new NullLogger())
        {
        }
    }

    public class MockFileProvider : IFileProvider
    {
        private IEnumerable<IFileInfo> _files;
        private Dictionary<string, IChangeToken> _changeTokens;

        public MockFileProvider()
        { }

        public MockFileProvider(params IFileInfo[] files)
        {
            _files = files;
        }

        public MockFileProvider(params KeyValuePair<string, IChangeToken>[] changeTokens)
        {
            _changeTokens = changeTokens.ToDictionary(
                changeToken => changeToken.Key,
                changeToken => changeToken.Value,
                StringComparer.Ordinal);
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            if (string.IsNullOrEmpty(subpath))
            {
                return new EnumerableDirectoryContents(_files);
            }

            var filesInFolder = _files.Where(f => f.Name.StartsWith(subpath, StringComparison.Ordinal));
            if (filesInFolder.Any())
            {
                return new EnumerableDirectoryContents(filesInFolder);
            }
            return new NotFoundDirectoryContents();
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            var file = _files.FirstOrDefault(f => f.Name == subpath);
            return file ?? new NotFoundFileInfo(subpath);
        }

        public IChangeToken Watch(string filter)
        {
            if (_changeTokens != null && _changeTokens.ContainsKey(filter))
            {
                return _changeTokens[filter];
            }
            return NoopChangeToken.Singleton;
        }
    }

    internal class EnumerableDirectoryContents : IDirectoryContents
    {
        private readonly IEnumerable<IFileInfo> _entries;

        public EnumerableDirectoryContents(IEnumerable<IFileInfo> entries)
        {
            _entries = entries;
        }

        public bool Exists
        {
            get { return true; }
        }

        public IEnumerator<IFileInfo> GetEnumerator()
        {
            return _entries.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _entries.GetEnumerator();
        }
    }

    internal class NotFoundDirectoryContents : IDirectoryContents
    {
        public NotFoundDirectoryContents()
        {
        }

        public bool Exists
        {
            get { return false; }
        }

        public IEnumerator<IFileInfo> GetEnumerator()
        {
            return Enumerable.Empty<IFileInfo>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    internal class NotFoundFileInfo : IFileInfo
    {
        private readonly string _name;

        public NotFoundFileInfo(string name)
        {
            _name = name;
        }

        public bool Exists
        {
            get { return false; }
        }

        public bool IsDirectory
        {
            get { return false; }
        }

        public DateTimeOffset LastModified
        {
            get { return DateTimeOffset.MinValue; }
        }

        public long Length
        {
            get { return -1; }
        }

        public string Name
        {
            get { return _name; }
        }

        public string PhysicalPath
        {
            get { return null; }
        }

        public Stream CreateReadStream()
        {
            throw new FileNotFoundException(string.Format("The file {0} does not exist.", Name));
        }
    }

    internal class NoopChangeToken : IChangeToken
    {
        public static NoopChangeToken Singleton { get; } = new NoopChangeToken();

        private NoopChangeToken()
        {
        }

        public bool HasChanged => false;

        public bool ActiveChangeCallbacks => false;

        public IDisposable RegisterChangeCallback(Action<object> callback, object state)
        {
            return EmptyDisposable.Instance;
        }
    }

    internal class EmptyDisposable : IDisposable
    {
        public static EmptyDisposable Instance { get; } = new EmptyDisposable();

        private EmptyDisposable()
        {
        }

        public void Dispose()
        {
        }
    }

    public class MockFileInfo : IFileInfo
    {
        public MockFileInfo(string name)
        {
            Name = name;
        }

        public bool Exists
        {
            get { return true; }
        }

        public bool IsDirectory { get; set; }

        public DateTimeOffset LastModified { get; set; }

        public long Length { get; set; }

        public string Name { get; }

        public string PhysicalPath { get; set; }

        public Stream CreateReadStream()
        {
            throw new NotImplementedException();
        }
    }
}