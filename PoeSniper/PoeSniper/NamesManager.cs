﻿using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace PoeSniper
{
    public class NamesManager
    {
        private const string _modNamesFile = @"data/modNames.json";
        private const string _weaponTypesFile = @"data/weaponTypes.json";

        private Logger _logger;

        public List<string> ModNames { get; set; }
        public List<string> WeaponTypes { get; set; }

        private bool _newModNames;

        public NamesManager(Logger logger)
        {
            _logger = logger;
        }

        public void Initialize()
        {
            var modNamesJson = File.ReadAllText(_modNamesFile);
            ModNames = JsonConvert.DeserializeObject<List<string>>(modNamesJson);
            if (ModNames == null)
            {
                ModNames = new List<string>();
            }

            var weaponTypesJson = File.ReadAllText(_weaponTypesFile);
            WeaponTypes = JsonConvert.DeserializeObject<List<string>>(weaponTypesJson);
            if (WeaponTypes == null)
            {
                WeaponTypes = new List<string>();
            }
        }

        public void AddModName(string modName)
        {
            if (modName != null && !ModNames.Contains(modName))
            {
                if (!_newModNames)
                {
                    _logger.Information("");
                }

                ModNames.Add(modName);
                _newModNames = true;

                _logger.Information("Adding new mod: '" + modName + "'");
            }
        }

        public void SaveModNames()
        {
            if (_newModNames)
            {
                var modNamesJson = JsonConvert.SerializeObject(ModNames, Formatting.Indented);
                File.WriteAllText(_modNamesFile, modNamesJson);

                _newModNames = false;
            }
        }

        public void VerifyWeaponType(string weaponType)
        {
            if (!WeaponTypes.Contains(weaponType))
            {
                _logger.Warning("Found new weapon type: " + weaponType);
                WeaponTypes.Add(weaponType);

                var weaponTypesJson = JsonConvert.SerializeObject(WeaponTypes, Formatting.Indented);
                File.WriteAllText(_weaponTypesFile, weaponTypesJson);
            }
        }
    }
}
