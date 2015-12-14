using Newtonsoft.Json.Linq;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.DependencyInjection;

namespace Orchard.ContentManagement.MetaData
{
    public interface IContentDefinitionWriter : IDependency
    {
        JToken Export(ContentTypeDefinition typeDefinition);
        JToken Export(ContentPartDefinition partDefinition);
    }
}