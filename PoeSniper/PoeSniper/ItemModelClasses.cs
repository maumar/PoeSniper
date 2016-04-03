using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoeSniper
{
    public class Item
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Base { get; set; }
        public string League { get; set; }
        public Rarity Rarity { get; set; }
        public bool IsIdentified { get; set; }
        public bool IsCorrupted { get; set; }
        public int Quality { get; set; }
        public string Note { get; set; }

        public List<ItemMod> ImplicitMods { get; set; }
        public List<ItemMod> ExplicitMods { get; set; }
        //public List<GemSocket> Sockets { get; set; }

        public ItemPrice Price { get; set; }
        public StashTab StashTab { get; set; }

        // map properties
        public int MapTier { get; set; }
        public int ItemQuantity { get; set; }
        public int ItemRarity { get; set; }

        // gem properties
        public int Level { get; set; }
        public bool IsMaxLevel { get; set; }

        //armor properties
        public int Armour { get; set; }
        public int Evasion { get; set; }
        public int EnergyShield { get; set; }
        public int ChanceToBlock { get; set; }
        public int ArmourWithMaxQuality { get; set; }
        public int EvasionWithMaxQuality { get; set; }
        public int EnergyShieldWithMaxQuality { get; set; }

        // weapon properties
        public string WeaponType { get; set; }
        public decimal PhysicalDamage { get; set; }
        public decimal FireDamage { get; set; }
        public decimal ColdDamage { get; set; }
        public decimal LightningDamage { get; set; }
        public decimal ChaosDamage { get; set; }
        public decimal CriticalStrikeChance { get; set; }
        public decimal AttacksPerSecond { get; set; }
        public decimal PhysicalDps { get; set; }
        public decimal ElementalDps { get; set; }
        public decimal Dps { get; set; }
        public decimal PhysicalDpsWithMaxQuality { get; set; }
        public decimal DpsWithMaxQuality { get; set; }
    }

    public class ItemMod
    {
        public string Name { get; set; }
        public decimal? Value { get; set; }
    }

    public class StashTab
    {
        public string AccountName { get; set; }
        public string CharacterName { get; set; }
        public string TabName { get; set; }
    }

    public class ItemPrice
    {
        public Currency Currency { get; set; }
        public decimal Value { get; set; }
        public PriceType Type { get; set; }
    }

    public enum Rarity
    {
        Normal,
        Magic,
        Rare,
        Unique,
    }

    public enum Currency
    {
        Alchemy,
        Alteration,
        Blessed,
        Chance,
        Chaos,
        Chisel,
        Chromatic,
        Divine,
        Exalted,
        Fusing,
        GCP,
        Jeweller,
        Mirror,
        Scour,
        Regret,
        Regal,
        Transmutation,
        Vaal,
    }

    public enum PriceType
    {
        Unknown = 0,
        Buyout = 1,
        FixedPrice = 2,
        CurrentOffer = 3,
    }
}
