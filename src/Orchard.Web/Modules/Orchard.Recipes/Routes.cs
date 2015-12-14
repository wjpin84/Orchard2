using System.Collections.Generic;
using Orchard.Hosting.Web.Routing.Routes;

namespace Orchard.Recipes {
    public class Routes : IRouteProvider {
        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                new RouteDescriptor {
                    Route = new Route(
                        "Recipes",
                        "Recipes/Status/{executionId}",
                        defaults:  new
                            {
                                area = "Orchard.Recipes",
                                controller = "Recipes",
                                action = "RecipeExecutionStatus"
                            },
                        dataTokens:  new
                            {
                                area = "Orchard.Recipes"
                            }
                        )
                }
            };
        }
    }
}