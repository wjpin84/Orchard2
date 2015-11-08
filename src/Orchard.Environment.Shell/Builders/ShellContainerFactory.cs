using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Shell.Builders.Models;
using DryIoc;
using DryIoc.Extensions.DependencyInjection;

#if DNXCORE50
using System.Reflection;
#endif

namespace Orchard.Environment.Shell.Builders {
    public class ShellContainerFactory : IShellContainerFactory {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly IContainer _container;
        private readonly ILoggerFactory _loggerFactory;

        public ShellContainerFactory(IServiceProvider serviceProvider,
            IContainer container,
            ILoggerFactory loggerFactory) {
            _serviceProvider = serviceProvider;
            _container = container;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<ShellContainerFactory>();
        }

        public IServiceProvider CreateContainer(ShellSettings settings, ShellBlueprint blueprint) {
            IServiceCollection serviceCollection = new ServiceCollection();
            
            serviceCollection.AddInstance(settings);
            serviceCollection.AddInstance(blueprint.Descriptor);
            serviceCollection.AddInstance(blueprint);

            // Sure this is right?
            serviceCollection.AddInstance(_loggerFactory);

            using (var scope = _container.OpenScope()) {
                foreach (var dependency in blueprint.Dependencies
                    .Where(t => typeof(IModule).IsAssignableFrom(t.Type))) {

                    scope.Register(typeof(IModule), dependency.Type);
                }

                foreach (var service in scope.Resolve<IServiceProvider>().GetServices<IModule>()) {
                    service.Configure(serviceCollection);
                }
            }
            
            foreach (var dependency in blueprint.Dependencies
                .Where(t => !typeof(IModule).IsAssignableFrom(t.Type))) {
                foreach (var interfaceType in dependency.Type.GetInterfaces()
                    .Where(itf => typeof(IDependency).IsAssignableFrom(itf))) {
                    _logger.LogDebug("Type: {0}, Interface Type: {1}", dependency.Type, interfaceType);

                    if (typeof(ISingletonDependency).IsAssignableFrom(interfaceType)) {
                        serviceCollection.AddSingleton(interfaceType, dependency.Type);
                    }
                    else if (typeof(IUnitOfWorkDependency).IsAssignableFrom(interfaceType)) {
                        serviceCollection.AddScoped(interfaceType, dependency.Type);
                    }
                    else if (typeof(ITransientDependency).IsAssignableFrom(interfaceType)) {
                        serviceCollection.AddTransient(interfaceType, dependency.Type);
                    }
                    else {
                        serviceCollection.AddScoped(interfaceType, dependency.Type);
                    }
                }
            }

            var nestedScope = _container.OpenScope();
            nestedScope.Populate(serviceCollection);
            return nestedScope.Resolve<IServiceProvider>();
        }
    }
}