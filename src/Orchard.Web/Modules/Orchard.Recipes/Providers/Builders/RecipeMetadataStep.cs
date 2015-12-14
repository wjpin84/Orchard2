using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Orchard.ContentManagement;
using Orchard.Environment.Recipes.Models;
using Orchard.Environment.Recipes.Services;
using Orchard.Recipes.ViewModels;

namespace Orchard.Recipes.Providers.Builders {
    public class RecipeMetadataStep : RecipeBuilderStep {
        public RecipeMetadataStep(ILoggerFactory loggerFactory) : base(loggerFactory) {
        }

        public override string Name {
            get { return "RecipeMetadata"; }
        }

        public override LocalizedString DisplayName {
            get { return T("Recipe Metadata"); }
        }

        public override LocalizedString Description {
            get { return T("Provides additional information about the recipe."); }
        }

        public override int Priority { get { return 1000; } }
        public override int Position { get { return -1000; } }

        public string RecipeName { get; set; }
        public string RecipeDescription { get; set; }
        public string RecipeAuthor { get; set; }
        public string RecipeWebsite { get; set; }
        public string RecipeTags { get; set; }
        public string RecipeCategory { get; set; }
        public string RecipeVersion { get; set; }
        public bool IsSetupRecipe { get; set; }

        public override dynamic BuildEditor(dynamic shapeFactory) {
            return UpdateEditor(shapeFactory, null);
        }

        public override dynamic UpdateEditor(dynamic shapeFactory, IUpdateModel updater) {
            var viewModel = new SetupRecipeStepViewModel {
                RecipeAuthor = "Nick Mayne"
            };

            if (updater != null && updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                RecipeName = viewModel.RecipeName;
                RecipeDescription = viewModel.RecipeDescription;
                RecipeAuthor = viewModel.RecipeAuthor;
                RecipeWebsite = viewModel.RecipeWebsite;
                RecipeTags = viewModel.RecipeTags;
                RecipeCategory = viewModel.RecipeCategory;
                RecipeVersion = viewModel.RecipeVersion;
                IsSetupRecipe = viewModel.IsSetupRecipe;
            }

            return shapeFactory.EditorTemplate(TemplateName: "BuilderSteps/RecipeMetadata", Model: viewModel, Prefix: Prefix);
        }

        public override void Configure(RecipeBuilderStepConfigurationContext context) {
            RecipeName = context.ConfigurationElement.Value<string>("Name");
            RecipeDescription = context.ConfigurationElement.Value<string>("Description");
            RecipeAuthor = context.ConfigurationElement.Value<string>("Author");
            RecipeWebsite = context.ConfigurationElement.Value<string>("Website");
            RecipeTags = context.ConfigurationElement.Value<string>("Tags");
            RecipeCategory = context.ConfigurationElement.Value<string>("Category");
            RecipeVersion = context.ConfigurationElement.Value<string>("Version");
            IsSetupRecipe = context.ConfigurationElement.Value<bool>("IsSetupRecipe");
        }

        public override void Build(BuildContext context) {
            var recipeElement = context.RecipeDocument["Orchard"]["Recipe"];
            
            recipeElement["Name"] = RecipeName;
            recipeElement["Description"] = RecipeDescription;
            recipeElement["Author"] = RecipeAuthor;
            recipeElement["WebSite"] = RecipeWebsite;
            recipeElement["Tags"] = RecipeTags;
            recipeElement["Category"] = RecipeCategory;
            recipeElement["Version"] = RecipeVersion;
            recipeElement["IsSetupRecipe"] = IsSetupRecipe;
        }
    }
}