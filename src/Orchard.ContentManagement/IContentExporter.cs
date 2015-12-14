using Newtonsoft.Json.Linq;
using Orchard.DependencyInjection;

namespace Orchard.ContentManagement
{
    public interface IContentExporter : IDependency
    {
        JObject Export(ContentItem contentItem);
    }
}
