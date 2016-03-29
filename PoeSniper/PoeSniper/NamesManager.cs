using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace PoeSniper
{
    public class NamesManager
    {
        private const string _properyNamesFile = @"data/propertyNames.json";
        private const string _modNamesFile = @"data/modNames.json";

        private bool _newPropertyNames;
        private bool _newModNames;

        public List<string> PropertyNames { get; set; }
        public List<string> ModNames { get; set; }

        public void LoadNames()
        {
            var propertyNamesJson = File.ReadAllText(_properyNamesFile);
            PropertyNames = JsonConvert.DeserializeObject<List<string>>(propertyNamesJson);
            if (PropertyNames == null)
            {
                PropertyNames = new List<string>();
            }

            var modNamesJson = File.ReadAllText(_modNamesFile);
            ModNames = JsonConvert.DeserializeObject<List<string>>(modNamesJson);
            if (ModNames == null)
            {
                ModNames = new List<string>();
            }
        }

        public void SaveNames()
        {
            if (_newPropertyNames)
            {
                var propertyNamesJson = JsonConvert.SerializeObject(PropertyNames, Formatting.Indented);
                File.WriteAllText(_properyNamesFile, propertyNamesJson);

                _newPropertyNames = false;
            }

            if (_newModNames)
            {
                var modNamesJson = JsonConvert.SerializeObject(ModNames);
                File.WriteAllText(_modNamesFile, modNamesJson);

                _newModNames = false;
            }
        }

        public void AddNewPropertyNames(IEnumerable<JsonProperty> jsonProperties)
        {
            if (jsonProperties == null)
            {
                return;
            }

            var newPropertyNames = jsonProperties.Select(p => p.name).Where(p => !PropertyNames.Contains(p));
            if (newPropertyNames.Any())
            {
                PropertyNames.AddRange(newPropertyNames);
                _newPropertyNames = true;
            }
        }
    }
}
