using System.Collections.Generic;

namespace PoeSniper
{
    public class JsonSearch
    {
        public string searchDescription { get; set; }
        public string itemName { get; set; }
        public string league { get; set; }
        public string itemBase { get; set; }

        public string itemRarity { get; set; }
        public bool? identified { get; set; }
        public bool? corrupted { get; set; }
        public string socketsAndLinks { get; set; }
        public string socketColors { get; set; }

        public int NumberOfSockets { get; set; }
        public int NumberOfLinkedSockets { get; set; }
        public int NumberOfRedSockets { get; set; }
        public int NumberOfGreenSockets { get; set; }
        public int NumberOfBlueSockets { get; set; }

        public int? quality { get; set; }
        public string note { get; set; }

        public bool? buyoutOnly { get; set; }
        public decimal? maxPrice { get; set; }
        public Currency? maxPriceCurrency { get; set; }

        public int? minMapTier { get; set; }
        public int? maxMapTier { get; set; }
        public int? mapItemQuantity { get; set; }
        public int? mapItemRarity { get; set; }

        public int? gemMinGemLevel { get; set; }
        public bool? gemIsMaxLevel { get; set; }

        public int? armourMinArmour { get; set; }
        public int? armourMaxArmour { get; set; }
        public int? armourMinEvasion { get; set; }
        public int? armourMaxEvasion { get; set; }
        public int? armourMinEnergyShield { get; set; }
        public int? armourMaxEnergyShield { get; set; }
        public int? armourMinChanceToBlock { get; set; }
        public int? armourMaxChanceToBlock { get; set; }

        public string weaponType { get; set; }
        public decimal? weaponMinDps { get; set; }
        public decimal? weaponMaxDps { get; set; }
        public decimal? weaponMinPhysicalDps { get; set; }
        public decimal? weaponMaxPhysicalDps { get; set; }
        public decimal? weaponMinElementalDps { get; set; }
        public decimal? weaponMaxElementalDps { get; set; }
        public decimal? weaponMinCriticalStrikeChance { get; set; }
        public decimal? weaponMaxCriticalStrikeChance { get; set; }
        public decimal? weaponMinAttacksPerSecondDps { get; set; }
        public decimal? weaponMaxAttacksPerSecondDps { get; set; }

        public List<JsonSearchMod> implicitMods { get; set; }

        public List<JsonSearchMod> explicitMods { get; set; }
    }

    public class JsonSearchMod
    {
        public string modName { get; set; }
        public decimal? modValue { get; set; }
    }

    public class JsonExchangeRates
    {
        public Dictionary<Currency, string> exchangeRates { get; set; }
    }
}
