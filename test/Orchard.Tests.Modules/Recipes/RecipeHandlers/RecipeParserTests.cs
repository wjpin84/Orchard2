using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Environment.Recipes.Services;
using System;
using Xunit;
using Orchard.Recipes.Services;
using Microsoft.Extensions.Logging;

namespace Orchard.Tests.Modules.Recipes.RecipeHandlers
{
    public class RecipeParserTests
    {
        private IServiceProvider _services;

        public RecipeParserTests()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped<ILoggerFactory, StubLoggerFactory>();
            serviceCollection.AddTransient< IRecipeParser, RecipeParser>();

            _services = serviceCollection.BuildServiceProvider();
        }

        [Fact]
        public void ParsingRecipeYieldsUniqueIdsForSteps()
        {
            var recipeText = @"{
   'Bar': { },
   'Baz': { },
 }";
            var recipeParser = _services.GetService<IRecipeParser>();
            var recipe = recipeParser.ParseRecipe(recipeText);

            // Assert that each step has a unique ID.
            Assert.True(recipe.RecipeSteps.GroupBy(x => x.Id).All(y => y.Count() == 1));
        }
    }
}
