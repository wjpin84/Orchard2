using Microsoft.AspNet.Builder;
using Orchard.DependencyInjection;
using System.Collections.Generic;

namespace Orchard.Hosting.Web.Routing.Routes
{
    public interface IRoutePublisher : ISingletonDependency
    {
        void Publish(IEnumerable<RouteDescriptor> routes, RequestDelegate pipeline);
    }
}