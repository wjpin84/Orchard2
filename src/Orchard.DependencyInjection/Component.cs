using Microsoft.Extensions.Logging;
using Orchard.Localization;

namespace Orchard.DependencyInjection
{
    public abstract class Component : IDependency
    {
        protected Component(ILogger logger)
        {
            T = NullLocalizer.Instance;
            Logger = logger;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }
    }
}