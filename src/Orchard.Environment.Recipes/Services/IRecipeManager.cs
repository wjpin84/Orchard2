using Orchard.Environment.Recipes.Models;
using System.Threading.Tasks;

namespace Orchard.Environment.Recipes.Services
{
    public interface IRecipeManager
    {
        Task<string> ExecuteAsync(Recipe recipe);
        Task ExecuteRecipeStepAsync(string executionId, Recipe recipe, RecipeStep recipeStep);
    }
}