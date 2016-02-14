using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Recipes.Models;
using Orchard.Environment.Recipes.Services;
using Orchard.FileSystem;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.Recipes.Services
{
    public class RecipeInfoHarvester : IRecipeHarvester
    {
        private readonly IExtensionManager _extensionManager;
        private readonly IRecipeParser _recipeInfoParser;
        private readonly IOrchardFileSystem _fileSystem;
        private readonly ILogger _logger;

        public RecipeInfoHarvester(
            IExtensionManager extensionManager,
            IRecipeParser recipeInfoParser,
            IOrchardFileSystem fileSystem,
            ILogger<RecipeInfoHarvester> logger)
        {
            _extensionManager = extensionManager;
            _recipeInfoParser = recipeInfoParser;
            _fileSystem = fileSystem;
            _logger = logger;
        }

        public async Task<IEnumerable<Recipe>> HarvestRecipesAsync()
        {
            return await Task.FromResult(
                _extensionManager
                    .AvailableExtensions()
                    .SelectMany(HarvestRecipes));
        }

        public async Task<IEnumerable<Recipe>> HarvestRecipesAsync(string extensionId)
        {
            var extension = _extensionManager.GetExtension(extensionId);
            if (extension != null)
            {
                return await Task.FromResult(HarvestRecipes(extension));
            }

            _logger.LogError("Could not discover recipes because module '{0}' was not found.", extensionId);
            return Enumerable.Empty<Recipe>();
        }

        private IEnumerable<Recipe> HarvestRecipes(ExtensionDescriptor extension)
        {
            var recipeLocation = _fileSystem.Combine(extension.Location, extension.Id, "Recipes");

            Matcher matcher = new Matcher(System.StringComparison.OrdinalIgnoreCase);
            matcher.AddInclude("*.recipe.json");
            var recipeFiles = _fileSystem.ListFiles(recipeLocation, matcher);

            foreach (var recipeFile in recipeFiles) {
                yield return
                    _recipeInfoParser.ParseRecipe(recipeFile);
            }
        }
    }
}