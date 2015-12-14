using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Orchard.DependencyInjection;
using Orchard.Environment.Recipes.Models;
using Orchard.Environment.Recipes.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Orchard.Recipes.Services
{
    public class RecipeParser : Component, IRecipeParser
    {
        public RecipeParser(ILoggerFactory loggerFactory) : base(loggerFactory) { }

        public Recipe ParseRecipe(JObject recipeDocument)
        {
            var recipe = new Recipe();
            var recipeSteps = new List<RecipeStep>();
            var stepId = 0;

            foreach (JProperty element in recipeDocument.Descendants().Where(x => x.Type == JTokenType.Property))
            {
                if (element.Name == "Recipe")
                {
                    recipe.Name = element.Value<string>("Name");
                    recipe.Description = element.Value<string>("Description");
                    recipe.Author = element.Value<string>("Author");
                    recipe.WebSite = element.Value<string>("WebSite");
                    recipe.Version = element.Value<string>("Version");
                    recipe.IsSetupRecipe = element.Value<bool>("IsSetupRecipe");
                    recipe.ExportUtc = element.Value<DateTime?>("ExportUtc");
                    recipe.Category = element.Value<string>("Category");
                    recipe.Tags = element.Value<string>("Tags");
                }
                // Recipe step.
                else
                {
                    var recipeStep = new RecipeStep(
                        id: (++stepId).ToString(CultureInfo.InvariantCulture),
                        recipeName: recipe.Name,
                        name: element.Name,
                        step: element);
                    recipeSteps.Add(recipeStep);
                }
            }

            recipe.RecipeSteps = recipeSteps;

            return recipe;
        }
    }
}