using System.Collections.Generic;
using System.Linq;
using Orchard.Environment.Recipes.Models;
using Orchard.DependencyInjection;
using Orchard.Environment.Recipes.Services;
using Microsoft.Extensions.Logging;

namespace Orchard.Recipes.Providers.RecipeHandlers {
    /// <summary>
    /// Delegates execution of the step to the appropriate recipe execution step implementation.
    /// </summary>
    public class RecipeExecutionStepHandler : Component, IRecipeHandler {
        private readonly IEnumerable<IRecipeExecutionStep> _recipeExecutionSteps;
        public RecipeExecutionStepHandler(IEnumerable<IRecipeExecutionStep> recipeExecutionSteps,
            ILoggerFactory loggerFactory) : base(loggerFactory) {
            _recipeExecutionSteps = recipeExecutionSteps;
        }

        public void ExecuteRecipeStep(RecipeContext recipeContext) {
            var executionStep = _recipeExecutionSteps.FirstOrDefault(x => x.Names.Contains(recipeContext.RecipeStep.Name));
            var recipeExecutionContext = new RecipeExecutionContext {ExecutionId = recipeContext.ExecutionId, RecipeStep = recipeContext.RecipeStep};

            if (executionStep != null) {
                Logger.LogInformation("Executing recipe step '{0}'.", recipeContext.RecipeStep.Name);
                executionStep.Execute(recipeExecutionContext);
                Logger.LogInformation("Finished executing recipe step '{0}'.", recipeContext.RecipeStep.Name);
                recipeContext.Executed = true;
            }
        }
    }
}