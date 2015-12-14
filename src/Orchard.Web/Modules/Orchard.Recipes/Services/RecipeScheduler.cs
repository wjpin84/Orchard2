using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.DependencyInjection;
using Orchard.Environment.Recipes.Events;
using Orchard.Environment.Recipes.Services;
using Orchard.Environment.Shell;
using Orchard.Environment.Shell.Descriptor;
using Orchard.Environment.Shell.State;
using System;
using System.Collections.Generic;

namespace Orchard.Recipes.Services
{
    public class RecipeScheduler : Component, IRecipeScheduler, IRecipeSchedulerEventHandler
    {
        private readonly IProcessingEngine _processingEngine;
        private readonly ShellSettings _shellSettings;
        private readonly IShellDescriptorManager _shellDescriptorManager;
        private readonly IServiceProvider _serviceLocator;
        private readonly IShellDescriptorManagerEventHandler _events;

        private readonly ContextState<string> _executionIds = new ContextState<string>("executionid");

        public RecipeScheduler(
            IProcessingEngine processingEngine,
            ShellSettings shellSettings,
            IShellDescriptorManager shellDescriptorManager,
            IServiceProvider serviceLocator,
            IShellDescriptorManagerEventHandler events,
            ILoggerFactory loggerFactory) : base(loggerFactory)
        {
            _processingEngine = processingEngine;
            _shellSettings = shellSettings;
            _shellDescriptorManager = shellDescriptorManager;
            _serviceLocator = serviceLocator;
            _events = events;
        }

        public void ScheduleWork(string executionId)
        {
            var shellDescriptor = _shellDescriptorManager.GetShellDescriptor();
            // TODO: this task entry may need to become appdata folder backed if it isn't already
            _processingEngine.AddTask(
                _shellSettings,
                shellDescriptor,
                "IRecipeSchedulerEventHandler.ExecuteWork",
                new Dictionary<string, object> { { "executionId", executionId } });
        }

        public void ExecuteWork(string executionId)
        {
            _executionIds.SetState(executionId);
            try
            {
                // todo: this callback should be guarded against concurrency by the IProcessingEngine
                var scheduleMore = _serviceLocator.GetService<IRecipeStepExecutor>().ExecuteNextStep(executionId);
                if (scheduleMore)
                {
                    Logger.LogInformation("Scheduling next step of recipe.");
                    ScheduleWork(executionId);
                }
                else
                {
                    Logger.LogInformation("All recipe steps executed; restarting shell.");
                    // https://github.com/OrchardCMS/Orchard/issues/3672
                    // Because recipes execute in their own workcontext, we need to restart the shell, as signaling a cache won't work across workcontexts.
                    _events.Changed(_shellDescriptorManager.GetShellDescriptor(), _shellSettings.Name);
                }
            }
            finally
            {
                _executionIds.SetState(null);
            }
        }
    }
}