using Microsoft.Extensions.Logging;
using Orchard.DependencyInjection;
using Orchard.Environment.Recipes.Events;
using Orchard.Environment.Recipes.Models;
using Orchard.Environment.Recipes.Services;
using Orchard.Environment.Shell.State;
using Orchard.Events;
using System;
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
        private readonly ILogger _logger;

        private readonly ContextState<string> _executionIds = new ContextState<string>("executionid");

        public RecipeManager(
            IRecipeStepQueue recipeStepQueue,
            IRecipeScheduler recipeScheduler,
            IEventBus eventBus,
            ISession session,
            ILogger<RecipeManager> logger)
        {
            _recipeStepQueue = recipeStepQueue;
            _recipeScheduler = recipeScheduler;
            _eventBus = eventBus;
            _session = session;
            _logger = logger;
        }

        public async Task<string> ExecuteAsync(Recipe recipe)
        {
            var executionId = Guid.NewGuid().ToString("n");

            _executionIds.SetState(executionId);

            _logger.LogInformation("Executing recipe '{0}'.", recipe.Name);
            try
            {
                _eventBus
                    .NotifyAsync<IRecipeExecuteEventHandler>(x => x.ExecutionStart(executionId, recipe))
                    .Wait();

                foreach (var recipeStep in recipe.Steps)
                {
                    await ExecuteRecipeStepAsync(executionId, recipe, recipeStep);
                }
                await _recipeScheduler.ScheduleWork(executionId);

                return executionId;
            }
            finally
            {
                _executionIds.SetState(null);
            }
        }

        public async Task ExecuteRecipeStepAsync(string executionId, Recipe recipe, RecipeStep recipeStep)
        {
            _recipeStepQueue.Enqueue(executionId, recipe, recipeStep);
            _session.Save(new RecipeStepResult
            {
                ExecutionId = executionId,
                RecipeName = recipe.Name,
                StepId = recipeStep.Id,
                StepName = recipeStep.Name
            });

            await Task.CompletedTask;
        }
    }
}