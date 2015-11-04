using DryIoc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace DryIoc.Extensions.DependencyInjection {
    public static class DryIocRegistration {
        public static void Populate(
                    this IContainer builder,
                    IEnumerable<ServiceDescriptor> descriptors) {
            builder.Register<IServiceProvider, DryIocServiceProvider>();
            builder.Register<IServiceScopeFactory, DryIocServiceScopeFactory>();

            Register(builder, descriptors);
        }

        private static void Register(
                    IContainer builder,
                    IEnumerable<ServiceDescriptor> descriptors) {

            foreach (var descriptor in descriptors) {
                var reuse = GetReuse(descriptor.Lifetime);

                if (descriptor.ImplementationType != null) {
                    builder
                        .Register(
                            descriptor.ServiceType,
                            descriptor.ImplementationType,
                            reuse,
                            setup: Setup.With(openResolutionScope: true));
                }
                else if (descriptor.ImplementationFactory != null) {
                    builder
                        .RegisterDelegate(descriptor.ServiceType, (context) => {
                            var serviceProvider = context.Resolve<IServiceProvider>();
                            return descriptor.ImplementationFactory(serviceProvider);
                        }, 
                        reuse,
                        setup: Setup.With(openResolutionScope: true));
                }
                else {
                    builder
                        .RegisterInstance(
                            descriptor.ServiceType, 
                            descriptor.ImplementationInstance,
                            reuse);
                }
            }
        }

        private static IReuse GetReuse(ServiceLifetime lifetime) {
            IReuse reuse;
            switch (lifetime) {
                case ServiceLifetime.Singleton:
                    reuse = Reuse.Singleton;
                    break;
                case ServiceLifetime.Scoped:
                    reuse = Reuse.InResolutionScope;
                    break;
                case ServiceLifetime.Transient:
                    reuse = Reuse.Transient;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
            }
            return reuse;
        }
    }

    class DryIocServiceProvider : IServiceProvider {
        private readonly IResolver _componentContext;

        public DryIocServiceProvider(IResolver componentContext) {
            _componentContext = componentContext;
        }

        public object GetService(Type serviceType) {
            return _componentContext.Resolve(serviceType, true);
        }
    }

    class DryIocServiceScopeFactory : IServiceScopeFactory {
        private readonly IContainer _lifetimeScope;

        public DryIocServiceScopeFactory(IContainer lifetimeScope) {
            _lifetimeScope = lifetimeScope;
        }

        public IServiceScope CreateScope() {
            return new DryIocServiceScope(_lifetimeScope.OpenScope());
        }

        class DryIocServiceScope : IServiceScope {
            private readonly IContainer _lifetimeScope;

            public DryIocServiceScope(IContainer lifetimeScope) {
                _lifetimeScope = lifetimeScope;
                ServiceProvider = _lifetimeScope.Resolve<IServiceProvider>();
            }

            public IServiceProvider ServiceProvider { get; }

            public void Dispose() => _lifetimeScope.Dispose();
        }
    }
}