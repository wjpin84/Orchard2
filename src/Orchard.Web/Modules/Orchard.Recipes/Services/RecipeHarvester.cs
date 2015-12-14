using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Recipes.Models;
using Orchard.Environment.Recipes.Services;
using Microsoft.Extensions.Logging;
using Orchard.DependencyInjection;
using Orchard.FileSystem;

namespace Orchard.Recipes.Services
{
    public class RecipeHarvester : Component, IRecipeHarvester
    {
        private readonly IExtensionManager _extensionManager;
        private readonly IClientFolder _webSiteFolder;
        private readonly IRecipeParser _recipeParser;

        public RecipeHarvester(
            IExtensionManager extensionManager,
            IClientFolder webSiteFolder,
            IRecipeParser recipeParser,
            ILoggerFactory loggerFactory) : base(loggerFactory)
        {
            _extensionManager = extensionManager;
            _webSiteFolder = webSiteFolder;
            _recipeParser = recipeParser;
        }

        public IEnumerable<Recipe> HarvestRecipes()
        {
            return _extensionManager.AvailableExtensions().SelectMany(HarvestRecipes);
        }

        public IEnumerable<Recipe> HarvestRecipes(string extensionId)
        {
            var extension = _extensionManager.GetExtension(extensionId);
            if (extension != null)
            {
                return HarvestRecipes(extension);
            }

            Logger.LogError("Could not discover recipes because module '{0}' was not found.", extensionId);
            return Enumerable.Empty<Recipe>();
        }

        private IEnumerable<Recipe> HarvestRecipes(ExtensionDescriptor extension)
        {
            var recipes = new List<Recipe>();

            var recipeLocation = Path.Combine(extension.Location, extension.Id, "Recipes");
            var recipeFiles = _webSiteFolder.ListFiles(recipeLocation, true);

            recipeFiles.Where(r => r.EndsWith(".recipe.json", StringComparison.OrdinalIgnoreCase)).ToList().ForEach(r =>
            {
                try
                {
                    recipes.Add(_recipeParser.ParseRecipe(_webSiteFolder.ReadFile(r)));
                }
                catch (Exception ex)
                {
                    Logger.LogError(string.Format("Error while parsing recipe file '{0}'.", r), ex);
                }
            });

            return recipes;
        }
    }
}