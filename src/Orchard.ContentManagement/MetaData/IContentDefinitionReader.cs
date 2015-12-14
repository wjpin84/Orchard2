using Newtonsoft.Json.Linq;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.DependencyInjection;

namespace Orchard.ContentManagement.MetaData
{
    public interface IContentDefinitionReader : IDependency
    {
        void Merge(JToken source, ContentTypeDefinitionBuilder builder);
        void Merge(JToken source, ContentPartDefinitionBuilder builder);
    }

    public static class ContentDefinitionReaderExtensions
    {
        public static ContentTypeDefinition Import(this IContentDefinitionReader reader, JToken source)
        {
            var target = new ContentTypeDefinitionBuilder();
            reader.Merge(source, target);
            return target.Build();
        }
    }
}