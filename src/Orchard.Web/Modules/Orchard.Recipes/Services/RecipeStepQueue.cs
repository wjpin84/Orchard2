using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Orchard.Environment.Recipes.Models;
using Orchard.Environment.Recipes.Services;
using Orchard.FileSystem.AppData;
using System.IO;
using System.Linq;

namespace Orchard.Recipes.Services
{
    public class RecipeStepQueue : IRecipeStepQueue
    {
        private readonly ILogger _logger;
        private readonly IAppDataFolder _appDataFolder;

        private readonly string _recipeQueueFolder = "RecipeQueue";

        public RecipeStepQueue(ILogger<RecipeStepQueue> logger,
            IAppDataFolder appDataFolder) {
            _logger = logger;
            _appDataFolder = appDataFolder;
        }

        public void Enqueue(string executionId, Recipe recipe, RecipeStep step)
        {
            _logger.LogInformation("Enqueuing recipe step '{0}'.", step.Name);
            var recipeStepElement = new JObject();
            recipeStepElement.Add(new JProperty("id", step.Id));
            recipeStepElement.Add(new JProperty("recipename", recipe.Name));
            recipeStepElement.Add(new JProperty("name", step.Name));
            recipeStepElement.Add(new JProperty("step", step.Step));

            if (_appDataFolder.DirectoryExists(_appDataFolder.Combine(_recipeQueueFolder, executionId)))
            {
                int stepIndex = GetLastStepIndex(executionId) + 1;
                _appDataFolder.CreateFile(_appDataFolder.Combine(_recipeQueueFolder, executionId + Path.DirectorySeparatorChar + stepIndex),
                                          recipeStepElement.ToString());
            }
            else {
                _appDataFolder.CreateFile(
                    _appDataFolder.Combine(_recipeQueueFolder, executionId + Path.DirectorySeparatorChar + "0"),
                    recipeStepElement.ToString());
            }
        }

        public RecipeStep Dequeue(string executionId)
        {
            _logger.LogInformation("Dequeuing recipe steps.");
            var executionFolderPath = _appDataFolder.Combine(_recipeQueueFolder, executionId);
            if (!_appDataFolder.DirectoryExists(executionFolderPath))
            {
                return null;
            }

            RecipeStep recipeStep = null;
            int stepIndex = GetFirstStepIndex(executionId);

            if (stepIndex >= 0)
            {
                var stepPath = _appDataFolder.Combine(_recipeQueueFolder, executionId + Path.DirectorySeparatorChar + stepIndex);

                var stepElement = JObject.Parse(_appDataFolder.ReadFile(stepPath));
                var stepName = stepElement.Value<string>("name");
                var recipeName = stepElement.Value<string>("recipename");
                var stepId = stepElement.Value<string>("id");
                
                _logger.LogInformation("Dequeuing recipe step '{0}'.", stepName);
                recipeStep = new RecipeStep(id: stepId, recipeName: recipeName, name: stepName, step: stepElement["step"]);
                _appDataFolder.DeleteFile(stepPath);
            }

            if (stepIndex < 0)
            {
                _appDataFolder.DeleteFile(executionFolderPath);
            }

            return recipeStep;
        }

        private int GetFirstStepIndex(string executionPath)
        {
            var stepFiles = _appDataFolder.ListFiles(executionPath);
            if (!stepFiles.Any())
                return -1;
            var currentSteps = stepFiles.Select(stepFile => int.Parse(stepFile.Name.Substring(stepFile.Name.LastIndexOf('/') + 1))).ToList();
            currentSteps.Sort();
            return currentSteps[0];
        }

        private int GetLastStepIndex(string executionPath)
        {
            int lastIndex = -1;
            var stepFiles = _appDataFolder.ListFiles(executionPath);
            // we always have only a handful of steps.
            foreach (var stepFile in stepFiles)
            {
                int stepOrder = int.Parse(stepFile.Name.Substring(stepFile.Name.LastIndexOf('/') + 1));
                if (stepOrder > lastIndex)
                    lastIndex = stepOrder;
            }

            return lastIndex;
        }
    }
}
