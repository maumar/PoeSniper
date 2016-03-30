using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Model;

namespace PoeSniper
{
    public class ItemProcessor
    {
        private List<string> _leagues;
        private NamesManager _namesManager;

        private readonly string[] _armourProperties = new[] 
        {
            "Armour",
            "Evasion",
            "Energy Shield"
        };

        public ItemProcessor(List<string> leagues, NamesManager namesManager)
        {
            _leagues = leagues;
            _namesManager = namesManager;
        }

        public List<Item> ProcessItems(JsonStashes jsonStashes)
        {
            var items = new List<Item>();
            foreach (var jsonStash in jsonStashes.stashes)
            {
                var league = jsonStash.items.FirstOrDefault()?.league;
                if (!_leagues.Contains(league))
                {
                    continue;
                }

                foreach (var jsonItem in jsonStash.items)
                {
                    var item = ProcessItem(jsonItem);
                    if (item != null)
                    {
                        items.Add(item);
                    }
                }
            }

            _namesManager.SaveModNames();

            return items;
        }

        private Item ProcessItem(JsonItem jsonItem)
        {
            var item = ProcessGenericItemProperties(jsonItem);

            var isGem = jsonItem.Name == "" && jsonItem.properties != null && jsonItem.properties.Any(p => p.name == "Level");
            var isArmor = jsonItem.properties != null && jsonItem.properties.Any(p => _armourProperties.Contains(p.name));
            var isWeapon = jsonItem.properties != null && jsonItem.properties.Any(p => p.name == "Attacks per Second");
            if (isGem)
            {
                item = ProcessGem(item, jsonItem);
            }
            else if (isArmor)
            {
                item = ProcessArmor(item, jsonItem);
            }
            else if (isWeapon)
            {
                item = ProcessWeapon(item, jsonItem);
            }

            if (!isGem)
            {
                ProcessMods(item, jsonItem);
            }

            return item;
        }

        private Item ProcessGenericItemProperties(JsonItem jsonItem)
        {
            var name = ExtractItemName(jsonItem);
            var quality = ExtractIntegerProperty(jsonItem, "Quality");

            var item = new Item();
            item.Id = jsonItem.id;
            item.Name = name;
            item.League = jsonItem.league;
            item.IsIdentified = jsonItem.identified;
            item.IsCorrupted = jsonItem.corrupted;
            item.Quality = quality;

            return item;
        }

        private Item ProcessGem(Item gem, JsonItem jsonItem)
        {
            var isMaxLevel = false;
            var level = 1;
            var levelProperty = (string)jsonItem.properties.Where(p => p.name == "Level").FirstOrDefault()?.values?[0]?[0];
            if (levelProperty != null)
            {
                isMaxLevel = levelProperty.Contains("(Max)");

                if (!int.TryParse(levelProperty.Replace("(Max)", ""), out level))
                {
                    Logger.Error("Couldn't parse gem level: '" + levelProperty + "'");
                }
            }

            gem.Level = level;
            gem.IsMaxLevel = isMaxLevel;

            return gem;
        }

        private Item ProcessArmor(Item armor, JsonItem jsonItem)
        {
            var armour = ExtractIntegerProperty(jsonItem, "Armour");
            var evasion = ExtractIntegerProperty(jsonItem, "Evasion");
            var energyShield = ExtractIntegerProperty(jsonItem, "Energy Shield");
            var chanceToBlock = ExtractIntegerProperty(jsonItem, "Chance to Block");

            armor.Armour = armour;
            armor.EvasionRating = evasion;
            armor.EnergyShield = energyShield;
            armor.ChanceToBlock = chanceToBlock;

            return armor;
        }


        private void ProcessMods(Item item, JsonItem jsonItem)
        {
            foreach (var jsonExplicitMod in jsonItem.explicitMods)
            {

            }
        }


        private Item ProcessWeapon(Item weapon, JsonItem jsonItem)
        {
            var weaponType = (string)jsonItem.properties.FirstOrDefault().name;
            if (string.IsNullOrEmpty(weaponType))
            {
                Logger.Error("Weapon type not found. Item name: " + weapon.Name);
            }

            _namesManager.VerifyWeaponType(weaponType);

            var attacksPerSecond = ExtractDecimalProperty(jsonItem, "Attacks per Second");
            var physicalDamage = ExtractRangeProperty(jsonItem, "Physical Damage");
            var chaosDamage = ExtractRangeProperty(jsonItem, "Chaos Damage");
            var criticalStrikeChance = ExtractDecimalProperty(jsonItem, "Critical Strike Chance");

            weapon.WeaponType = weaponType;
            weapon.AttacksPerSecond = attacksPerSecond;
            weapon.PhysicalDamage = physicalDamage;
            weapon.ChaosDamage = chaosDamage;
            weapon.CriticalStrikeChance = criticalStrikeChance;

            ProcessElementalDamage(weapon, jsonItem);

            return weapon;
        }

        private Item ProcessElementalDamage(Item weapon, JsonItem jsonItem)
        {
            var elementalDamageProperty = jsonItem.properties.Where(p => p.name == "Elemental Damage").FirstOrDefault();
            if (elementalDamageProperty != null)
            {
                foreach (var value in elementalDamageProperty.values)
                {
                    var damageValue = ExtractRangePropertyValue((string)value[0], "Elemental Damage");
                    switch((long)value[1])
                    {
                        case 4:
                            weapon.FireDamage = damageValue;
                            break;

                        case 5:
                            weapon.ColdDamage = damageValue;
                            break;

                        case 6:
                            weapon.LightningDamage = damageValue;
                            break;

                        default:
                            Logger.Error("Invalid Elemental Damage type: " + value[1]);
                            break;
                    }
                }
            }

            return weapon;
        }

        private string ExtractItemName(JsonItem jsonItem)
        {
            var nameStartIndex = jsonItem.Name.LastIndexOf(">");
            var name = jsonItem.Name.Substring(nameStartIndex + 1);

            return string.IsNullOrEmpty(name) ? jsonItem.typeLine : name + " " + jsonItem.typeLine;
        }

        private int ExtractIntegerProperty(JsonItem jsonItem, string propertyName, int defaultValue = 0)
        {
            var propertyValue = defaultValue;
            if (jsonItem.properties != null)
            {
                var propertyString = (string)jsonItem.properties.Where(p => p.name == propertyName).FirstOrDefault()?.values?[0]?[0];
                if (propertyString != null)
                {
                    if (!int.TryParse(propertyString.Replace("%", ""), out propertyValue))
                    {
                        Logger.Error("Couldn't parse property. Name: '" + propertyName + "' Value: '" + propertyString + "'");
                    }
                }
            }

            return propertyValue;
        }

        private decimal ExtractDecimalProperty(JsonItem jsonItem, string propertyName, decimal defaultValue = 0.0M)
        {
            var propertyValue = defaultValue;
            if (jsonItem.properties != null)
            {
                var propertyString = (string)jsonItem.properties.Where(p => p.name == propertyName).FirstOrDefault()?.values?[0]?[0];
                if (propertyString != null)
                {
                    if (!decimal.TryParse(propertyString.Replace("%", ""), out propertyValue))
                    {
                        Logger.Error("Couldn't parse property. Name: '" + propertyName + "' Value: '" + propertyString + "'");
                    }
                }
            }

            return propertyValue;
        }

        private decimal ExtractRangeProperty(JsonItem jsonItem, string propertyName, decimal defaultValue = 0.0M)
        {
            var propertyValue = defaultValue;
            if (jsonItem.properties != null)
            {
                var propertyString = (string)jsonItem.properties.Where(p => p.name == propertyName).FirstOrDefault()?.values?[0]?[0];
                if (propertyString != null)
                {
                    propertyValue = ExtractRangePropertyValue(propertyString, propertyName);
                }
            }

            return propertyValue;
        }

        private decimal ExtractRangePropertyValue(string propertyString, string propertyName)
        {
            var rangeValues = propertyString.Split(new[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
            if (rangeValues.Count() != 2)
            {
                Logger.Error("Couldn't parse range property. Name: '" + propertyName + "' Value: '" + propertyString + "'");
            }

            int minValue;
            if (!int.TryParse(rangeValues[0], out minValue))
            {
                Logger.Error("Couldn't parse min value of a range property. Name: '" + propertyName + "' Value: '" + rangeValues[0] + "'");
            }

            int maxValue;
            if (!int.TryParse(rangeValues[1], out maxValue))
            {
                Logger.Error("Couldn't parse max value of a range property. Name: '" + propertyName + "' Value: '" + rangeValues[1] + "'");
            }

            return (maxValue + minValue) / 2.0M;
        }
    }
}
