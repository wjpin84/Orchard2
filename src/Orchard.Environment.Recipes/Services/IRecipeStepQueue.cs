using Orchard.Environment.Recipes.Models;

namespace Orchard.Environment.Recipes.Services
{
    public interface IRecipeStepQueue
    {
        void Enqueue(string executionId, Recipe recipe, RecipeStep step);
        RecipeStep Dequeue(string executionId);
    }
}