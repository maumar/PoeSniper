using System;
using System.Collections.Generic;
using System.Linq;
using Model;

namespace PoeSniper
{
    public class ItemProcessor
    {
        private List<string> _leagues;
        private NamesManager _namesManager;

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

            _namesManager.SaveNames();

            return items;
        }

        private Item ProcessItem(JsonItem jsonItem)
        {
            if (jsonItem.Name == "" && jsonItem.properties != null && jsonItem.properties.Any(p => p.name == "Level"))
            {
                return ProcessGem(jsonItem);
            }

            return null;
        }

        private Gem ProcessGem(JsonItem jsonItem)
        {
            if (jsonItem.properties != null)
            {
                // first property describes tags, skipping
                _namesManager.AddNewPropertyNames(jsonItem.properties.Skip(1));
            }

            var isMaxLevel = false;
            var level = 1;
            var levelProperty = (string)jsonItem.properties.Where(p => p.name == "Level").FirstOrDefault()?.values?[0]?[0];
            if (levelProperty != null)
            {
                isMaxLevel = levelProperty.Contains("(Max)");

                if (!int.TryParse(levelProperty.Replace("(Max)", ""), out level))
                {
                    Console.WriteLine("ERROR - Couldn't parse gem level: '" + levelProperty + "'");
                }
            }

            var quality = 0;
            var qualityProperty = (string)jsonItem.properties.Where(p => p.name == "Quality").FirstOrDefault()?.values?[0]?[0];
            if (qualityProperty != null)
            {
                if (!int.TryParse(qualityProperty.Replace("%", ""), out quality))
                {
                    Console.WriteLine("ERROR - Couldn't parse gem quality: '" + qualityProperty + "'");
                }
            }

            var gem = new Gem
            {
                Name = jsonItem.typeLine,
                Level = level,
                IsMaxLevel = isMaxLevel,
                Quality = quality,
                IsCorrupted = jsonItem.corrupted,
            };

            return gem;
        }
    }
}
