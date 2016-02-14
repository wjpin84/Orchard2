using Microsoft.AspNet.FileProviders;
using Orchard.Environment.Recipes.Models;

namespace Orchard.Environment.Recipes.Services
{
    public interface IRecipeParser
    {
        Recipe ParseRecipe(IFileInfo fileInfo);
    }
}