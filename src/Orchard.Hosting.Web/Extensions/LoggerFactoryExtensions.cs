using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.Logging;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Loaders;
using System;
using System.Linq;
using Orchard.Environment.Shell.Builders;
using DryIoc;

#if DNXCORE50
using System.Reflection;
#endif

namespace Orchard.Hosting.Extensions {
    public static class LoggerFactoryExtensions {
        public static ILoggerFactory AddOrchardLogging(
            this ILoggerFactory loggingFactory, 
            IServiceProvider serviceProvider) {
            /* TODO (ngm): Abstract this logger stuff outta here! */
            
            var loaders = serviceProvider.GetServices<IExtensionLoader>();
            var manager = serviceProvider.GetService<IExtensionManager>();

            var descriptor = manager.GetExtension("Orchard.Logging.Console");
            var entry = loaders.Select(loader => loader.Load(descriptor)).FirstOrDefault(x => x != null);
            var loggingInitiatorTypes = entry
                .Assembly
                .ExportedTypes
                .Where(et => typeof(ILoggingInitiator).IsAssignableFrom(et));

            using (var scope = new Container().OpenScope()) {
                foreach (var initiatorType in loggingInitiatorTypes) {
                    scope.Register(typeof(ILoggingInitiator), initiatorType);
                }

                foreach (var service in scope.Resolve<IServiceProvider>().GetServices<ILoggingInitiator>()) {
                    service.Initialize(loggingFactory);
                }
            }

            return loggingFactory;
        }
    }
}
