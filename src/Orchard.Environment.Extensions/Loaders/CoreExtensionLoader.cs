using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Orchard.DependencyInjection;
using Orchard.Environment.Extensions.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Orchard.FileSystem.AppData;

namespace Orchard.Environment.Extensions.Loaders
{
    public class CoreExtensionLoader : IExtensionLoader
    {
        private const string CoreAssemblyName = "Orchard.Core";
        private readonly IAssemblyLoaderContainer _loaderContainer;
        private readonly IExtensionAssemblyLoader _extensionAssemblyLoader;
        private readonly IOrchardFileSystem _orchardFileSystem;
        private readonly ILogger _logger;

        public CoreExtensionLoader(
            IAssemblyLoaderContainer container,
            IExtensionAssemblyLoader extensionAssemblyLoader,
            IOrchardFileSystem orchardFileSystem,
            ILogger<CoreExtensionLoader> logger)
        {
            _loaderContainer = container;
            _extensionAssemblyLoader = extensionAssemblyLoader;
            _orchardFileSystem = orchardFileSystem;
            _logger = logger;
        }

        public string Name => GetType().Name;

        public int Order => 10;

        public void ExtensionActivated(ExtensionLoadingContext ctx, ExtensionDescriptor extension)
        {
        }

        public void ExtensionDeactivated(ExtensionLoadingContext ctx, ExtensionDescriptor extension)
        {
        }

        public bool IsCompatibleWithModuleReferences(ExtensionDescriptor extension, IEnumerable<ExtensionProbeEntry> references)
        {
            return true;
        }

        public ExtensionEntry Load(ExtensionDescriptor descriptor)
        {
            if (!descriptor.Location.StartsWith("Core/"))
            {
                return null;
            }

            var plocation = _orchardFileSystem.MapPath("Core");

            using (_loaderContainer.AddLoader(_extensionAssemblyLoader.WithPath(plocation)))
            {
                var assembly = Assembly.Load(new AssemblyName(CoreAssemblyName));

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Loaded referenced extension \"{0}\": assembly name=\"{1}\"", descriptor.Name, assembly.FullName);
                }
                return new ExtensionEntry
                {
                    Descriptor = descriptor,
                    Assembly = assembly,
                    ExportedTypes = assembly.ExportedTypes.Where(x => IsTypeFromModule(x, descriptor))
                };
            }
        }

        public ExtensionProbeEntry Probe(ExtensionDescriptor descriptor)
        {
            return null;
        }

        public void ReferenceActivated(ExtensionLoadingContext context, ExtensionReferenceProbeEntry referenceEntry)
        {
        }

        public void ReferenceDeactivated(ExtensionLoadingContext context, ExtensionReferenceProbeEntry referenceEntry)
        {
        }
        private static bool IsTypeFromModule(Type type, ExtensionDescriptor descriptor)
        {
            return (type.Namespace + ".").StartsWith(CoreAssemblyName + "." + descriptor.Id + ".");
        }
    }
}