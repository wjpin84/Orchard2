using Orchard.Environment.Recipes.Models;
using Orchard.Environment.Recipes.Services;
using Orchard.Recipes.Models;
using Orchard.Recipes.Records;
using System;
using System.Linq;
using System.Threading.Tasks;
using YesSql.Core.Services;

namespace Orchard.Recipes.Services
{
    public class RecipeResultAccessor : IRecipeResultAccessor
    {
        private readonly ISession _session;

        public RecipeResultAccessor(ISession session)
        {
            _session = session;
        }

        public async Task<RecipeResult> GetResult(string executionId)
        {
            var records = await _session
                .QueryAsync<RecipeStepResultRecord, RecipeStepResultIndex>()
                .Where(x => x.ExecutionId == executionId)
                .List();

            if (!records.Any())
                throw new Exception(string.Format("No records were found in the database for recipe execution ID {0}.", executionId));

            return new RecipeResult()
            {
                ExecutionId = executionId,
                Steps =
                    from record in records
                    select new RecipeStepResult
                    {
                        RecipeName = record.RecipeName,
                        StepName = record.StepName,
                        IsCompleted = record.IsCompleted,
                        IsSuccessful = record.IsSuccessful,
                        ErrorMessage = record.ErrorMessage
                    }
            };
        }
    }
}