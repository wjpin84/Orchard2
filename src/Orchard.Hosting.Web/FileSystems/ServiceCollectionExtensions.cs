using Microsoft.Extensions.DependencyInjection;
using Orchard.FileSystem.AppData;
using Orchard.Hosting.FileSystems;

namespace Orchard.Hosting.FileSystem
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWebFileSystem(this IServiceCollection services)
        {
            services.AddSingleton<IOrchardFileSystem, WebFileSystem>();

            return services;
        }
    }
}