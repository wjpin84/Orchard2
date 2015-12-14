//using System.Linq;
//using System.Xml;
//using System.Xml.Linq;
//using Orchard.ContentManagement.MetaData.Models;
//using Newtonsoft.Json.Linq;
//using System;

//namespace Orchard.ContentManagement.MetaData.Services
//{
//    /// <summary>
//    /// Abstraction to manage settings metadata on a content.
//    /// </summary>
//    public class SettingsFormatter : ISettingsFormatter
//    {
//        /// <summary>
//        /// Maps an JSON element to a settings dictionary.
//        /// </summary>
//        /// <param name="element">The JSON element to be mapped.</param>
//        /// <returns>The settings dictionary.</returns>
//        public SettingsDictionary Map(JToken element)
//        {
//            if (element == null)
//            {
//                return new SettingsDictionary();
//            }

//            throw new NotImplementedException("Comon Mayne... half job");
//        }

//        /// <summary>
//        /// Maps a settings dictionary to an JSON element.
//        /// </summary>
//        /// <param name="settingsDictionary">The settings dictionary.</param>
//        /// <returns>The JSON element.</returns>
//        public JToken Map(SettingsDictionary settingsDictionary)
//        {
//            return new JProperty("settings", settingsDictionary
//                    .Where(kv => kv.Value != null)
//                    .Select(value => new JProperty(value.Key, value.Value)));
//        }
//    }
//}