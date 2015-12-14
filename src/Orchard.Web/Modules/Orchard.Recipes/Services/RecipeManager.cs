using Microsoft.Extensions.Logging;
using Orchard.DependencyInjection;
using Orchard.Environment.Recipes.Events;
using Orchard.Environment.Recipes.Models;
using Orchard.Environment.Recipes.Services;
using Orchard.Environment.Shell.State;
using Orchard.Events;
using Orchard.Recipes.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using YesSql.Core.Services;

namespace Orchard.Recipes.Services
{
    public class RecipeManager : Component, IRecipeManager
    {
        private readonly IRecipeStepQueue _recipeStepQueue;
        private readonly IRecipeScheduler _recipeScheduler;
        private readonly IEventBus _eventBus;
        private readonly ISession _session;

        private readonly ContextState<string> _executionIds = new ContextState<string>("executionid");

        public RecipeManager(
            IRecipeStepQueue recipeStepQueue,
            IRecipeScheduler recipeScheduler,
            IEventBus eventBus,
            ISession session,
            ILoggerFactory loggerFactory) : base(loggerFactory)
        {

            _recipeStepQueue = recipeStepQueue;
            _recipeScheduler = recipeScheduler;
            _eventBus = eventBus;
            _session = session;
        }

        public async Task<string> ExecuteAsync(Recipe recipe)
        {
            if (recipe == null)
            {
                throw new ArgumentNullException("recipe");
            }

            if (!recipe.RecipeSteps.Any())
            {
                Logger.LogInformation("Recipe '{0}' contains no steps. No work has been scheduled.");
                return null;
            }

            var executionId = Guid.NewGuid().ToString("n");

            _executionIds.SetState(executionId);

            try
            {
                Logger.LogInformation("Executing recipe '{0}'.", recipe.Name);
                await _eventBus.NotifyAsync<IRecipeExecuteEventHandler>(x => x.ExecutionStart(executionId, recipe));

                foreach (var recipeStep in recipe.RecipeSteps)
                {
                    _recipeStepQueue.Enqueue(executionId, recipeStep);
                    _session.Save(new RecipeStepResultRecord
                    {
                        ExecutionId = executionId,
                        RecipeName = recipe.Name,
                        StepId = recipeStep.Id,
                        StepName = recipeStep.Name
                    });
                }
                _recipeScheduler.ScheduleWork(executionId);

                return executionId;
            }
            finally
            {
                _executionIds.SetState(null);
            }
        }
    }
}