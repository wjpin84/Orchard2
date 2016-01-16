using Microsoft.Extensions.Caching.Memory;

namespace Orchard.Tests.Stubs
{
    public class StubMemoryCache : IMemoryCache
    {
        public IEntryLink CreateLinkingScope()
        {
            return null;
        }

        public void Dispose()
        {
        }

        public void Remove(object key)
        {
        }

        public object Set(object key, object value, MemoryCacheEntryOptions options)
        {
            return value;
        }

        public bool TryGetValue(object key, out object value)
        {
            value = null;
            return true;
        }
    }
}