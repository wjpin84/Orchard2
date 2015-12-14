using Microsoft.Extensions.Logging;
using Orchard.DependencyInjection;
using Orchard.Environment.Recipes.Models;
using Orchard.Environment.Shell.Descriptor;
using System.Threading.Tasks;

namespace Orchard.Environment.Recipes.Services
{
    public class RecipeExecutor : Component, IRecipeExecutor
    {
        private readonly IRecipeManager _recipeManager;
        private readonly IShellDescriptorManager _shellDescriptorManager;

        public RecipeExecutor(
            IRecipeParser recipeParser,
            IRecipeManager recipeManager,
            IShellDescriptorManager shellDescriptorManager,
            ILoggerFactory loggerFactory) : base(loggerFactory)
        {
            _recipeManager = recipeManager;
            _shellDescriptorManager = shellDescriptorManager;
        }

        public async Task<string> Execute(Recipe recipe)
        {
            var executionId = await _recipeManager.ExecuteAsync(recipe);

            // Only need to update the shell if work was actually done.
            if (executionId != null)
                UpdateShell();

            return executionId;
        }

        private void UpdateShell()
        {
            var descriptor = _shellDescriptorManager.GetShellDescriptor();
            _shellDescriptorManager.UpdateShellDescriptor(descriptor.SerialNumber, descriptor.Features, descriptor.Parameters);
        }
    }
}