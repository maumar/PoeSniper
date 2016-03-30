namespace Model
{
    public class Item
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Base { get; set; }
        public string League { get; set; }
        public bool IsIdentified { get; set; }
        public bool IsCorrupted { get; set; }
        public int Quality { get; set; }

        //public List<ItemProperty> ImplicitProperties { get; set; }
        //public List<ItemProperty> ExplicitProperties { get; set; }
        //public List<GemSocket> Sockets { get; set; }

        public StashTab StashTab { get; set; }

        //public Currency? PriceCurrency { get; set; }
        public decimal? PriceAmount { get; set; }

        // gem properties
        public int Level { get; set; }
        public bool IsMaxLevel { get; set; }

        //armor properties
        public int Armour { get; set; }
        public int EvasionRating { get; set; }
        public int EnergyShield { get; set; }
        public int ChanceToBlock { get; set; }
        public int ArmourWithMaxQuality { get; set; }
        public int EvasionRatingWithMaxQuality { get; set; }
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
}
