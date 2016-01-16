using System.Reflection;

namespace Orchard.Tests.Hosting.Environment.Extensions
{
    internal class EmbeddedFileProvider
    {
        private Assembly assembly;
        private object @namespace;

        public EmbeddedFileProvider(Assembly assembly, object @namespace)
        {
            this.assembly = assembly;
            this.@namespace = @namespace;
        }
    }
}