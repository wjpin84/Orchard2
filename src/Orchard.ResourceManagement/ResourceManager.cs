using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Orchard.ResourceManagement
{
    public class ResourceManager : IResourceManager
    {
        private readonly Dictionary<ResourceTypeName, RequireSettings> _required = new Dictionary<ResourceTypeName, RequireSettings>();
        private List<LinkEntry> _links;
        private Dictionary<string, MetaEntry> _metas;
        private readonly Dictionary<string, IList<ResourceRequiredContext>> _builtResources;
        private readonly IEnumerable<IResourceManifestProvider> _providers;
        private ResourceManifest _dynamicManifest;
        private List<string> _headScripts;
        private List<string> _footScripts;

        private readonly IResourceManifestState _resourceManifestState;

        public ResourceManager(
            IEnumerable<IResourceManifestProvider> resourceProviders,
            IResourceManifestState resourceManifestState)
        {
            _resourceManifestState = resourceManifestState;
            _providers = resourceProviders;

            _builtResources = new Dictionary<string, IList<ResourceRequiredContext>>(StringComparer.OrdinalIgnoreCase);
        }

        public IEnumerable<ResourceManifest> ResourceManifests
        {
            get
            {
                if (_resourceManifestState.ResourceManifests == null)
                {
                    var builder = new ResourceManifestBuilder();
                    foreach (var provider in _providers)
                    {
                        provider.BuildManifests(builder);
                    }
                    _resourceManifestState.ResourceManifests = builder.ResourceManifests;
                }
                return _resourceManifestState.ResourceManifests;
            }
        }

        public ResourceManifest InlineManifest
        {
            get
            {
                if(_dynamicManifest == null)
                {
                    _dynamicManifest = new ResourceManifest();
                }

                return _dynamicManifest;
            }
        }

        public RequireSettings RegisterResource(string resourceType, string resourceName)
        {
            if (resourceType == null)
            {
                throw new ArgumentNullException(nameof(resourceType));
            }

            if (resourceName == null)
            {
                throw new ArgumentNullException(nameof(resourceName));
            }

            RequireSettings settings;
            var key = new ResourceTypeName(resourceType, resourceName);
            if (!_required.TryGetValue(key, out settings))
            {
                settings = new RequireSettings { Type = resourceType, Name = resourceName };
                _required[key] = settings;
            }
            _builtResources[resourceType] = null;
            return settings;
        }

        public RequireSettings Include(string resourceType, string resourcePath, string resourceDebugPath)
        {
            return RegisterUrl(resourceType, resourcePath, resourceDebugPath, null);
        }

        public RequireSettings RegisterUrl(string resourceType, string resourcePath, string resourceDebugPath, string relativeFromPath)
        {
            if (resourceType == null)
            {
                throw new ArgumentNullException(nameof(resourceType));
            }
            if (resourcePath == null)
            {
                throw new ArgumentNullException(nameof(resourcePath));
            }

            // ~/ ==> convert to absolute path (e.g. /orchard/..)
            if (resourcePath.StartsWith("~/", StringComparison.Ordinal))
            {
                // For tilde slash paths, drop the leading ~ to make it work with the underlying IFileProvider.
                resourcePath = resourcePath.Substring(1);
            }
            if (resourceDebugPath != null && resourceDebugPath.StartsWith("~/", StringComparison.Ordinal))
            {
                // For tilde slash paths, drop the leading ~ to make it work with the underlying IFileProvider.
                resourceDebugPath = resourceDebugPath.Substring(1);
            }

            return RegisterResource(resourceType, resourcePath).Define(d => d.SetUrl(resourcePath, resourceDebugPath));
        }

        public void RegisterHeadScript(string script)
        {
            if (_headScripts == null)
            {
                _headScripts = new List<string>();
            }

            _headScripts.Add(script);
        }

        public void RegisterFootScript(string script)
        {
            if (_footScripts == null)
            {
                _footScripts = new List<string>();
            }

            _footScripts.Add(script);
        }

        public void NotRequired(string resourceType, string resourceName)
        {
            if (resourceType == null)
            {
                throw new ArgumentNullException(nameof(resourceType));
            }

            if (resourceName == null)
            {
                throw new ArgumentNullException(nameof(resourceName));
            }

            var key = new ResourceTypeName(resourceType, resourceName);
            _builtResources[resourceType] = null;
            _required.Remove(key);
        }

        public ResourceDefinition FindResource(RequireSettings settings)
        {
            return FindResource(settings, true);
        }

        private ResourceDefinition FindResource(RequireSettings settings, bool resolveInlineDefinitions)
        {
            // find the resource with the given type and name
            // that has at least the given version number. If multiple,
            // return the resource with the greatest version number.
            // If not found and an inlineDefinition is given, define the resource on the fly
            // using the action.
            var name = settings.Name ?? "";
            var type = settings.Type;
            var resource = (from p in ResourceManifests
                            from r in p.GetResources(type)
                            where name.Equals(r.Key, StringComparison.OrdinalIgnoreCase)
                            let version = r.Value.Version != null ? new Version(r.Value.Version) : null
                            orderby version descending
                            select r.Value).FirstOrDefault();
            if (resource == null && _dynamicManifest != null)
            {
                resource = (from r in _dynamicManifest.GetResources(type)
                            where name.Equals(r.Key, StringComparison.OrdinalIgnoreCase)
                            let version = r.Value.Version != null ? new Version(r.Value.Version) : null
                            orderby version descending
                            select r.Value).FirstOrDefault();
            }
            if (resolveInlineDefinitions && resource == null)
            {
                // Does not seem to exist, but it's possible it is being
                // defined by a Define() from a RequireSettings somewhere.
                if (ResolveInlineDefinitions(settings.Type))
                {
                    // if any were defined, now try to find it
                    resource = FindResource(settings, false);
                }
            }
            return resource;
        }

        private bool ResolveInlineDefinitions(string resourceType)
        {
            bool anyWereDefined = false;
            foreach (var settings in ResolveRequiredResources(resourceType).Where(settings => settings.InlineDefinition != null))
            {
                // defining it on the fly
                var resource = FindResource(settings, false);
                if (resource == null)
                {
                    // does not already exist, so define it
                    resource = InlineManifest.DefineResource(resourceType, settings.Name).SetBasePath(settings.BasePath);
                    anyWereDefined = true;
                }
                settings.InlineDefinition(resource);
                settings.InlineDefinition = null;
            }
            return anyWereDefined;
        }

        private IEnumerable<RequireSettings> ResolveRequiredResources(string resourceType)
        {
            return _required.Where(r => r.Key.Type == resourceType).Select(r => r.Value);
        }

        public IEnumerable<LinkEntry> GetRegisteredLinks()
        {
            if (_links == null)
            {
                return Enumerable.Empty<LinkEntry>();
            }

            return _links.AsReadOnly();
        }

        public IEnumerable<MetaEntry> GetRegisteredMetas()
        {
            if(_metas == null)
            {
                return Enumerable.Empty<MetaEntry>();
            }

            return _metas.Values;
        }

        public IEnumerable<string> GetRegisteredHeadScripts()
        {
            return _headScripts == null ? Enumerable.Empty<string>() : _headScripts;
        }

        public IEnumerable<string> GetRegisteredFootScripts()
        {
            return _footScripts == null ? Enumerable.Empty<string>() : _footScripts;
        }

        public IEnumerable<ResourceRequiredContext> GetRequiredResources(string resourceType)
        {
            IList<ResourceRequiredContext> requiredResources;
            if (_builtResources.TryGetValue(resourceType, out requiredResources) && requiredResources != null)
            {
                return requiredResources;
            }
            var allResources = new OrderedDictionary();
            foreach (var settings in ResolveRequiredResources(resourceType))
            {
                var resource = FindResource(settings);
                if (resource == null)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, "A '{1}' named '{0}' could not be found.", settings.Name, settings.Type));
                }
                ExpandDependencies(resource, settings, allResources);
            }
            requiredResources = (from DictionaryEntry entry in allResources
                                 select new ResourceRequiredContext { Resource = (ResourceDefinition)entry.Key, Settings = (RequireSettings)entry.Value }).ToList();
            _builtResources[resourceType] = requiredResources;
            return requiredResources;
        }

        protected virtual void ExpandDependencies(ResourceDefinition resource, RequireSettings settings, OrderedDictionary allResources)
        {
            if (resource == null)
            {
                return;
            }
            // Settings is given so they can cascade down into dependencies. For example, if Foo depends on Bar, and Foo's required
            // location is Head, so too should Bar's location.
            // forge the effective require settings for this resource
            // (1) If a require exists for the resource, combine with it. Last settings in gets preference for its specified values.
            // (2) If no require already exists, form a new settings object based on the given one but with its own type/name.
            settings = allResources.Contains(resource)
                ? ((RequireSettings)allResources[resource]).Combine(settings)
                : new RequireSettings { Type = resource.Type, Name = resource.Name }.Combine(settings);
            if (resource.Dependencies != null)
            {
                var dependencies = from d in resource.Dependencies
                                   select FindResource(new RequireSettings { Type = resource.Type, Name = d });
                foreach (var dependency in dependencies)
                {
                    if (dependency == null)
                    {
                        continue;
                    }
                    ExpandDependencies(dependency, settings, allResources);
                }
            }
            allResources[resource] = settings;
        }

        public void RegisterLink(LinkEntry link)
        {
            if(_links == null)
            {
                _links = new List<LinkEntry>();
            }

            _links.Add(link);
        }

        public void RegisterMeta(MetaEntry meta)
        {
            if (meta == null)
            {
                return;
            }

            if(_metas == null)
            {
                _metas = new Dictionary<string, MetaEntry>();
            }

            var index = meta.Name ?? meta.HttpEquiv ?? "charset";

            _metas[index] = meta;
        }

        public void AppendMeta(MetaEntry meta, string contentSeparator)
        {
            if (meta == null)
            {
                return;
            }

            var index = meta.Name ?? meta.HttpEquiv;

            if (String.IsNullOrEmpty(index))
            {
                return;
            }

            if (_metas == null)
            {
                _metas = new Dictionary<string, MetaEntry>();
            }

            MetaEntry existingMeta;
            if (_metas.TryGetValue(index, out existingMeta))
            {
                meta = MetaEntry.Combine(existingMeta, meta, contentSeparator);
            }

            _metas[index] = meta;
        }

        private class ResourceTypeName : IEquatable<ResourceTypeName>
        {
            private readonly string _type;
            private readonly string _name;

            public string Type { get { return _type; } }
            public string Name { get { return _name; } }

            public ResourceTypeName(string resourceType, string resourceName)
            {
                _type = resourceType;
                _name = resourceName;
            }

            public bool Equals(ResourceTypeName other)
            {
                if (other == null)
                {
                    return false;
                }

                return _type.Equals(other._type) && _name.Equals(other._name);
            }

            public override int GetHashCode()
            {
                return _type.GetHashCode() << 17 + _name.GetHashCode();
            }

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.Append("(");
                sb.Append(_type);
                sb.Append(", ");
                sb.Append(_name);
                sb.Append(")");
                return sb.ToString();
            }
        }
    }
}
