using System.Collections.Generic;
using System.Linq;
using Orchard.DependencyInjection;
using Orchard.Services;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;

namespace Orchard.Environment.Recipes.Services
{
    public class RecipeBuilder : Component, IRecipeBuilder
    {
        private readonly IClock _clock;

        public RecipeBuilder(IClock clock,
            ILoggerFactory loggerFactory) : base(loggerFactory)
        {
            _clock = clock;
        }

        public JObject Build(IEnumerable<IRecipeBuilderStep> steps)
        {
            var context = new BuildContext
            {
                RecipeDocument = CreateRecipeRoot()
            };

            foreach (var step in steps.OrderByDescending(x => x.Priority))
            {
                step.Build(context);
            }

            return context.RecipeDocument;
        }

        private JObject CreateRecipeRoot()
        {
            dynamic recipeRoot = new JObject();
            recipeRoot._comment = T("Exported from Orchard").ToString();
            recipeRoot.ExportUtc = _clock.UtcNow;
            return recipeRoot;
        }
    }
}