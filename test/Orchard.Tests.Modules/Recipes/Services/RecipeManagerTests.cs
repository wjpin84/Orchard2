//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using Orchard.Environment.Extensions;
//using Orchard.Environment.Recipes.Services;
//using Orchard.Recipes.Services;

//namespace Orchard.Tests.Modules.Recipes.Services
//{
//    public class RecipeManagerTests
//    {
//        private IRecipeManager _recipeManager;
//        private IRecipeHarvester _recipeHarvester;
//        private IRecipeParser _recipeParser;

//        public RecipeManagerTests() {
//            var serviceCollection = new ServiceCollection();
//            serviceCollection.AddScoped<ILoggerFactory, StubLoggerFactory>();
//            serviceCollection.AddTransient<IRecipeParser, RecipeParser>();
//            serviceCollection.AddTransient<IRecipeHarvester, RecipeHarvester>();
//            serviceCollection.AddTransient<IRecipeStepExecutor, RecipeStepExecutor>();
//            serviceCollection.AddTransient<IExtensionManager, ExtensionManager>();



//        }
//    }
//}
