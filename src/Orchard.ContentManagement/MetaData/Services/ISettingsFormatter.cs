using Orchard.ContentManagement.MetaData.Models;
using Orchard.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace Orchard.ContentManagement.MetaData.Services
{
    /// <summary>
    /// Abstraction to manage settings metadata on a content.
    /// </summary>
    public interface ISettingsFormatter : IDependency
    {
        /// <summary>
        /// Maps an XML element to a settings dictionary.
        /// </summary>
        /// <param name="element">The JSON element to be mapped.</param>
        /// <returns>The settings dictionary.</returns>
        SettingsDictionary Map(JToken element);

        /// <summary>
        /// Maps a settings dictionary to an JSON element.
        /// </summary>
        /// <param name="settingsDictionary">The settings dictionary.</param>
        /// <returns>The XML element.</returns>
        JToken Map(SettingsDictionary settingsDictionary);
    }
}