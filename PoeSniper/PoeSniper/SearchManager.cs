using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace PoeSniper
{
    public class SearchManager
    {
        private const string _searchesDirectoryPath = "searches";

        private readonly Regex _socketsAndLinksRegex = new Regex(@"((?<sockets>\d)S)?((?<links>\d)L)?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private List<JsonSearch> _searches;
        private List<string> _foundItemIds;
        private Logger _logger;
        private PriceProcessor _priceProcessor;

        public SearchManager(Logger logger, PriceProcessor priceProcessor)
        {
            _searches = new List<JsonSearch>();
            _foundItemIds = new List<string>();

            _logger = logger;
            _priceProcessor = priceProcessor;
        }

        public void UpdateSearches()
        {
            string[] searchFileNames = new string[0];

            _logger.Information("Updating searches", newLine: false);

            try
            {
                searchFileNames = Directory.GetFiles(_searchesDirectoryPath);
            }
            catch (Exception ex)
            {
                _logger.Error("");
                _logger.Error("Couldn't access 'searches' folder");
                _logger.Error(ex.Message);

                return;
            }

            _searches = new List<JsonSearch>();
            foreach (var searchFileName in searchFileNames.Where(n => Path.GetFileName(n) != "templateSearch.json"))
            {
                try
                {
                    var searchJson = File.ReadAllText(searchFileName);
                    var search = JsonConvert.DeserializeObject<JsonSearch>(searchJson);

                    CalculateSocketsAndLinks(search);

                    _searches.Add(search);
                }
                catch (Exception ex)
                {
                    _logger.Error("");
                    _logger.Error("Couldn't load search file: '" + Path.GetFileName(searchFileName) + "'");
                    _logger.Error(ex.Message);
                }
            }

            _logger.Information("Found: " + _searches.Count);
        }

        private void CalculateSocketsAndLinks(JsonSearch search)
        {
            if (!string.IsNullOrEmpty(search.socketsAndLinks))
            {
                var socketsAndLinksMatch = _socketsAndLinksRegex.Match(search.socketsAndLinks);
                if (socketsAndLinksMatch.Success)
                {
                    search.NumberOfSockets = int.Parse("0" + socketsAndLinksMatch.Groups["sockets"].Value ?? "");
                    search.NumberOfLinkedSockets = int.Parse("0" + socketsAndLinksMatch.Groups["links"].Value ?? "");
                }
            }

            if (!string.IsNullOrEmpty(search.socketColors))
            {
                search.NumberOfRedSockets = search.socketColors.Where(s => s == 'r' || s == 'R').Count();
                search.NumberOfGreenSockets = search.socketColors.Where(s => s == 'g' || s == 'G').Count();
                search.NumberOfBlueSockets = search.socketColors.Where(s => s == 'b' || s == 'B').Count();
            }
        }

        public void Search(List<Item> items)
        {
            foreach (var search in _searches)
            {
                foreach (var item in items)
                {
                    if (!BasicPropertiesMet(search, item))
                    {
                        continue;
                    }

                    // TODO: gem properties
                    // TODO: map properties
                    // TODO: armor properties
                    // TODO: weapon properties

                    if (!ModsMet(search.implicitMods, item.ImplicitMods))
                    {
                        continue;
                    }

                    if (!ModsMet(search.explicitMods, item.ExplicitMods))
                    {
                        continue;
                    }

                    if (!_foundItemIds.Contains(item.Id))
                    {
                        _foundItemIds.Add(item.Id);
                        LogFoundItem(search, item);
                    }
                }
            }
        }

        private bool BasicPropertiesMet(JsonSearch search, Item item)
        {
            if (!CriterionMet(search.itemName, item.Name, exactMatch: false)
                || !CriterionMet(search.note, (item.Note ?? "") + " " + (item.StashTab.TabName ?? ""), exactMatch: false))
            {
                return false;
            }

            if (!CriterionMet(search.league, item.League)
                || !CriterionMet(search.league, item.League)
                || !CriterionMet(search.itemBase, item.Base))
            {
                return false;
            }

            if (!CriterionMet(search.identified, item.IsIdentified)
                || !CriterionMet(search.corrupted, item.IsCorrupted))
            {
                return false;
            }

            if (!CriterionMet(search.quality, item.Quality))
            {
                return false;
            }

            if (search.buyoutOnly != null && search.buyoutOnly.Value && item.Price == null)
            {
                return false;
            }

            if (search.maxPrice != null && search.maxPriceCurrency != null && item.Price != null)
            {
                var itemPriceValue = item.Price.Value;
                if (search.maxPriceCurrency.Value != item.Price.Currency)
                {
                    itemPriceValue = _priceProcessor.CalculateExchangeRate(itemPriceValue, item.Price.Currency, search.maxPriceCurrency.Value) ?? 0.0M;
                }

                if (search.maxPrice < itemPriceValue)
                {
                    return false;
                }
            }

            return true;
        }

        private bool ModsMet(List<JsonSearchMod> searchMods, List<ItemMod> itemMods)
        {
            if (searchMods != null && searchMods.Count > 0)
            {
                if (itemMods == null)
                {
                    return false;
                }

                foreach (var searchMod in searchMods)
                {
                    var matchingItemMod = itemMods.Where(m => m.Name == searchMod.modName).FirstOrDefault();
                    if (matchingItemMod == null)
                    {
                        return false;
                    }

                    if (searchMod.modValue != null)
                    {
                        if (matchingItemMod.Value == null || matchingItemMod.Value < searchMod.modValue)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private bool CriterionMet(string searchCriterion, string itemValue, bool exactMatch = true)
        {
            if (string.IsNullOrEmpty(searchCriterion))
            {
                return true;
            }

            return exactMatch
                ? itemValue != null && searchCriterion.ToLower() == itemValue.ToLower()
                : itemValue != null && itemValue.ToLower().Contains(searchCriterion.ToLower());
        }

        private bool CriterionMet(bool? searchCriterion, bool? itemValue)
        {
            return searchCriterion == null || searchCriterion == itemValue;
        }

        private bool CriterionMet(int? searchCriterion, int? itemValue)
        {
            return searchCriterion == null || searchCriterion == itemValue;
        }

        private bool CriterionMet(decimal? searchCriterion, decimal? itemValue)
        {
            return searchCriterion == null || searchCriterion == itemValue;
        }

        private bool ItemRarityMet(string searchRarity, Rarity itemRarity)
        {
            if (string.IsNullOrEmpty(searchRarity))
            {
                return true;
            }

            if (searchRarity.ToLower() == "normal" && itemRarity == Rarity.Normal)
            {
                return true;
            }

            if (searchRarity.ToLower() == "magic" && itemRarity == Rarity.Normal)
            {
                return true;
            }

            if (searchRarity.ToLower() == "rare" && itemRarity == Rarity.Normal)
            {
                return true;
            }

            if (searchRarity.ToLower() == "unique" && itemRarity == Rarity.Normal)
            {
                return true;
            }

            return false;
        }


        private void LogFoundItem(JsonSearch jsonSearch, Item item)
        {
            _logger.ItemFound("ITEM FOUND - " + jsonSearch.searchDescription);
            _logger.ItemFound("Item Name: " + item.Name + " (League: " + item.League + ")");
            _logger.ItemFound("");
            _logger.ItemFound("Account Name: " + item.StashTab.AccountName + " Character Name: " + item.StashTab.CharacterName);

            if (item.Price != null)
            {
                _logger.ItemFound("Price: " + item.Price.Type + " " + item.Price.Value + " " + item.Price.Currency + " ", newLine: false);
            }

            _logger.ItemFound("Stash | Note: " + (item.StashTab.TabName ?? "") + " | " + (item.Note ?? ""));
        }
    }
}
