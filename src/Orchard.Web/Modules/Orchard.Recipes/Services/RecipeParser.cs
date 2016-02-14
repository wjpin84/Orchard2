using Microsoft.AspNet.FileProviders;
using Newtonsoft.Json;
using Orchard.Environment.Recipes.Models;
using Orchard.Environment.Recipes.Services;
using Orchard.FileSystem;
using System.IO;

namespace Orchard.Recipes.Services
{
    public class RecipeParser : IRecipeParser
    {
        private readonly IOrchardFileSystem _fileSystem;

        public RecipeParser(
            IOrchardFileSystem fileSystem) {
            _fileSystem = fileSystem;
        }
        
        public Recipe ParseRecipe(IFileInfo fileInfo)
        {
            var serializer = new JsonSerializer();
            serializer.MaxDepth = 1;
            using (StreamReader streamReader = new StreamReader(fileInfo.CreateReadStream()))
            {
                using (JsonTextReader reader = new JsonTextReader(streamReader))
                {
                    var recipe = serializer.Deserialize<PhysicalRecipe>(reader);
                    recipe.FileInfo = fileInfo;
                    return recipe;
                }
            }
        }
    }
}