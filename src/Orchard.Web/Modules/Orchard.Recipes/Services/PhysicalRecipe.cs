using Microsoft.AspNet.FileProviders;
using Newtonsoft.Json;
using Orchard.Environment.Recipes.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace Orchard.Recipes.Services
{
    public class PhysicalRecipe : Recipe
    {
        public IFileInfo FileInfo { get; set; }

        public override IEnumerable<RecipeStep> Steps
        {
            get
            {
                var serializer = new JsonSerializer();
                using (StreamReader streamReader = new StreamReader(FileInfo.CreateReadStream()))
                {
                    using (JsonTextReader reader = new JsonTextReader(streamReader))
                    {
                        while (reader.Read())
                        {
                            yield return new RecipeStep("", Name, "", null);
                        }

                    }
                }
            }
            set
            {
                throw new NotSupportedException();
            }
        }
    }
}
