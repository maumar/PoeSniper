using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace PoeSniper
{
    public class ItemProcessor
    {
        private Logger _logger;
        private Settings _settings;
        private NamesManager _namesManager;
        private PropertyProcessor _propertyProcessor;
        private PriceProcessor _priceProcessor;
        private ArmorProcessor _armorProcessor;
        private WeaponProcessor _weaponProcessor;


        private readonly string[] _armourProperties = new[] 
        {
            "Armour",
            "Evasion",
            "Energy Shield"
        };

        private readonly Regex _damageRangeRegex = new Regex(@"(?<damageRange>\d+-\d+)", RegexOptions.Compiled);
        private readonly Regex _genericPropertyRegex = new Regex(@"(?<value>" + Regex.Escape("+") + @"?-?\d+" + Regex.Escape(".") + @"?\d*)", RegexOptions.Compiled);

        public ItemProcessor(Settings settings, NamesManager namesManager, Logger logger, PriceProcessor priceProcessor)
        {
            _settings = settings;
            _namesManager = namesManager;
            _logger = logger;
            _propertyProcessor = new PropertyProcessor(_logger);
            _priceProcessor = priceProcessor;
            _armorProcessor = new ArmorProcessor(_propertyProcessor);
            _weaponProcessor = new WeaponProcessor(_propertyProcessor, _namesManager, _logger);
        }

        public List<Item> ProcessItems(JsonStashes jsonStashes)
        {
            var items = new List<Item>();
            if (jsonStashes.stashes != null && jsonStashes.stashes.Count > 0)
            {
                _logger.Information(DateTime.Now + " Processing", newLine: false);

                var sw = new Stopwatch();
                sw.Start();

                foreach (var jsonStash in jsonStashes.stashes)
                {
                    var league = jsonStash.items.FirstOrDefault()?.league;
                    if (!_settings.leagues.Contains(league))
                    {
                        continue;
                    }

                    var stashTab = new StashTab
                    {
                        AccountName = jsonStash.accountName,
                        CharacterName = jsonStash.lastCharacterName,
                        TabName = jsonStash.stash
                    };

                    foreach (var jsonItem in jsonStash.items)
                    {
                        var item = ProcessItem(jsonItem);
                        if (item != null)
                        {
                            item.Price = _priceProcessor.ProcessPrice(jsonItem.note);
                            if (item.Price == null)
                            {
                                item.Price = _priceProcessor.ProcessPrice(jsonStash.stash);
                            }

                            items.Add(item);
                            item.StashTab = stashTab;
                        }
                    }
                }

                _namesManager.SaveModNames();

                _logger.Information(" Items: " + items.Count, newLine: false);
                _logger.Information(" | Done in " + sw.Elapsed);
            }

            if (items.Count < _settings.sleepTreshold)
            {
                _logger.Information("Indexing too fast. Sleeping for " + _settings.sleepTime + " seconds.");
                Thread.Sleep(_settings.sleepTime * 1000);
            }
        
            return items;
        }

        private Item ProcessItem(JsonItem jsonItem)
        {
            var item = ProcessGenericItemProperties(jsonItem);

            var isStackable = jsonItem.properties != null && jsonItem.properties.Any(p => p.name == "Stack Size");
            var isMap = jsonItem.properties != null && jsonItem.properties.Any(p => p.name == "Map Tier");
            var isGem = jsonItem.Name == "" && jsonItem.properties != null && jsonItem.properties.Any(p => p.name == "Level");
            var isArmor = jsonItem.properties != null && jsonItem.properties.Any(p => _armourProperties.Contains(p.name));
            var isWeapon = jsonItem.properties != null && jsonItem.properties.Any(p => p.name == "Attacks per Second");

            if (!isGem && !isMap && !isStackable)
            {
                ProcessMods(item, jsonItem.implicitMods);
                ProcessMods(item, jsonItem.explicitMods);
            }

            if (!isGem)
            {
                ProcessItemRarity(item, jsonItem);
            }
            else
            {
                item.Rarity = Rarity.Normal;
            }

            if (isMap)
            {
                item = ProcessMap(item, jsonItem);
            }
            else if (isGem)
            {
                item = ProcessGem(item, jsonItem);
            }
            else if (isArmor)
            {
                item = _armorProcessor.ProcessArmor(item, jsonItem);
            }
            else if (isWeapon)
            {
                item = _weaponProcessor.ProcessWeapon(item, jsonItem);
            }

            return item;
        }

        private Item ProcessGenericItemProperties(JsonItem jsonItem)
        {
            var name = ExtractItemName(jsonItem);
            var quality = _propertyProcessor.ExtractIntegerProperty(jsonItem, "Quality");

            var item = new Item();
            item.Id = jsonItem.id;
            item.Name = name;
            item.League = jsonItem.league;
            item.IsIdentified = jsonItem.identified;
            item.IsCorrupted = jsonItem.corrupted;
            item.Quality = quality;
            item.Note = jsonItem.note;

            return item;
        }

        private Item ProcessItemRarity(Item item, JsonItem jsonItem)
        {
            if (jsonItem.flavourText != null && jsonItem.flavourText.Count > 0)
            {
                item.Rarity = Rarity.Unique;
            }
            else if (jsonItem.explicitMods != null && jsonItem.explicitMods.Count > 2)
            {
                item.Rarity = Rarity.Rare;
            }
            else
            {
                item.Rarity = jsonItem.explicitMods != null && jsonItem.explicitMods.Count > 0
                    ? Rarity.Magic
                    : Rarity.Normal;
            }

            return item;
        }

        private Item ProcessMap(Item map, JsonItem jsonItem)
        {
            var mapTier = _propertyProcessor.ExtractIntegerProperty(jsonItem, "Map Tier");
            var itemQuantity = _propertyProcessor.ExtractIntegerProperty(jsonItem, "Item Quantity");
            var itemRarity = _propertyProcessor.ExtractIntegerProperty(jsonItem, "Item Rarity");
           
            map.MapTier = mapTier;
            map.ItemQuantity = itemQuantity;
            map.ItemRarity = itemRarity;

            return map;
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
                    _logger.Error("Couldn't parse gem level: '" + levelProperty + "'");
                }
            }

            gem.Level = level;
            gem.IsMaxLevel = isMaxLevel;
            gem.Rarity = Rarity.Normal;

            return gem;
        }

        private void ProcessMods(Item item, List<string> jsonItem)
        {
            item.ExplicitMods = new List<ItemMod>();
            if (jsonItem != null)
            {
                foreach (var jsonExplicitMod in jsonItem)
                {
                    if (_namesManager.ModNames.Contains(jsonExplicitMod))
                    {
                        item.ExplicitMods.Add(new ItemMod { Name = jsonExplicitMod, Value = null });
                    }
                    else
                    {
                        string modName;
                        decimal? modValue;

                        ProcessMod(jsonExplicitMod, out modName, out modValue);
                        _namesManager.AddModName(modName);

                        if (modName != null)
                        {
                            item.ExplicitMods.Add(new ItemMod { Name = modName, Value = modValue });
                        }
                    }
                }
            }
        }

        private void ProcessMod(string modString, out string modName, out decimal? modValue)
        {
            if (modString.StartsWith("<"))
            {
                modName = null;
                modValue = null;

                return;
            }

            var match = _damageRangeRegex.Match(modString);
            if (match.Success)
            {
                modName = modString.Replace(match.Value, "X");
                var range = match.Value.Split(new[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                modValue = (decimal.Parse(range[0]) + decimal.Parse(range[1])) / 2.0M;
            }
            else
            {
                match = _genericPropertyRegex.Match(modString);
                if (match.Success)
                {
                    var index = modString.IndexOf(match.Value);
                    modName = modString.Remove(index, match.Value.Length).Insert(index, "X");
                    modValue = decimal.Parse(match.Value);
                }
                else
                {
                    modName = modString;
                    modValue = null;
                }
            }
        }

        private string ExtractItemName(JsonItem jsonItem)
        {
            var nameStartIndex = jsonItem.Name.LastIndexOf(">");
            var name = jsonItem.Name.Substring(nameStartIndex + 1);

            return string.IsNullOrEmpty(name) ? jsonItem.typeLine : name + " " + jsonItem.typeLine;
        }
    }
}
