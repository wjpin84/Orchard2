using System.IO;
using System.Linq;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.Environment.Recipes.Services;
using Orchard.FileSystem.AppData;
using Microsoft.Extensions.Logging;
using Orchard.DependencyInjection;
using Orchard.Environment.Recipes.Models;
using Newtonsoft.Json.Linq;

namespace Orchard.Recipes.Services
{
    public class RecipeStepQueue : Component, IRecipeStepQueue
    {
        private readonly IAppDataFolder _appDataFolder;
        private readonly string _recipeQueueFolder = "RecipeQueue" + Path.DirectorySeparatorChar;

        public RecipeStepQueue(IAppDataFolder appDataFolder,
            ILoggerFactory loggerFactory) : base(loggerFactory)
        {
            _appDataFolder = appDataFolder;
        }

        public void Enqueue(string executionId, RecipeStep step)
        {
            Logger.LogInformation("Enqueuing recipe step '{0}'.", step.Name);
            var recipeStepElement = new JObject();
            recipeStepElement.Add("Id", step.Id);
            recipeStepElement.Add("RecipeName", step.RecipeName);
            recipeStepElement.Add("Name", step.Name);
            step.Step["RecipeStep"] = recipeStepElement;

            if (_appDataFolder.DirectoryExists(Path.Combine(_recipeQueueFolder, executionId)))
            {
                int stepIndex = GetLastStepIndex(executionId) + 1;
                _appDataFolder.CreateFile(Path.Combine(_recipeQueueFolder, executionId + Path.DirectorySeparatorChar + stepIndex),
                                          recipeStepElement.ToString());
            }
            else
            {
                _appDataFolder.CreateFile(
                    Path.Combine(_recipeQueueFolder, executionId + Path.DirectorySeparatorChar + "0"),
                    recipeStepElement.ToString());
            }
        }

        public RecipeStep Dequeue(string executionId)
        {
            Logger.LogInformation("Dequeuing recipe steps.");
            if (!_appDataFolder.DirectoryExists(Path.Combine(_recipeQueueFolder, executionId)))
            {
                return null;
            }
            RecipeStep recipeStep = null;
            int stepIndex = GetFirstStepIndex(executionId);
            if (stepIndex >= 0)
            {
                var stepPath = Path.Combine(_recipeQueueFolder, executionId + Path.DirectorySeparatorChar + stepIndex);
                // string to xelement
                var stepElement = JObject.Parse(_appDataFolder.ReadFile(stepPath));
                var stepName = stepElement["Name"].Value<string>();
                var stepId = stepElement["Id"].Value<string>();
                var recipeName = stepElement["RecipeName"].Value<string>();
                Logger.LogInformation("Dequeuing recipe step '{0}'.", stepName);
                recipeStep = new RecipeStep(id: stepId, recipeName: recipeName, name: stepName, step: stepElement[stepName]);
                _appDataFolder.DeleteFile(stepPath);
            }

            if (stepIndex < 0)
            {
                _appDataFolder.DeleteFile(Path.Combine(_recipeQueueFolder, executionId));
            }

            return recipeStep;
        }

        private int GetFirstStepIndex(string executionId)
        {
            var stepFiles = _appDataFolder.ListFiles(Path.Combine(_recipeQueueFolder, executionId));
            if (!stepFiles.Any())
                return -1;
            var currentSteps = stepFiles.Select(stepFile => int.Parse(stepFile.Name.Substring(stepFile.Name.LastIndexOf('/') + 1))).ToList();
            currentSteps.Sort();
            return currentSteps[0];
        }

        private int GetLastStepIndex(string executionId)
        {
            int lastIndex = -1;
            var stepFiles = _appDataFolder.ListFiles(Path.Combine(_recipeQueueFolder, executionId));
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