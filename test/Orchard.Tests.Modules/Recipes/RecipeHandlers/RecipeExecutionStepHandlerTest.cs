using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Recipes.Models;
using Orchard.Environment.Recipes.Services;
using Orchard.Recipes.Providers.RecipeHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.Tests.Modules.Recipes.RecipeHandlers
{
    public class RecipeExecutionStepHandlerTest
    {
        private readonly IServiceProvider _serviceProvider;

        public RecipeExecutionStepHandlerTest() {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddScoped<ILoggerFactory, StubLoggerFactory>();
            serviceCollection.AddSingleton<IRecipeExecutionStep, StubRecipeExecutionStep>();
            serviceCollection.AddSingleton<RecipeExecutionStepHandler>();

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }
    }

    public class StubRecipeExecutionStep : RecipeExecutionStep
    {

        public StubRecipeExecutionStep(
            ILogger<StubRecipeExecutionStep> logger) : base(logger)
        {
        }

        public override string Name
        {
            get { return "FakeRecipeStep"; }
        }

        public bool IsExecuted { get; set; }

        public override void Execute(RecipeExecutionContext context)
        {
            IsExecuted = true;
        }
    }
}
