using System.Linq;

namespace PoeSniper
{
    public class ArmorProcessor
    {
        private PropertyProcessor _propertyProcessor;

        public ArmorProcessor(PropertyProcessor propertyProcessor)
        {
            _propertyProcessor = propertyProcessor;
        }

        public Item ProcessArmor(Item armor, JsonItem jsonItem)
        {
            var armour = _propertyProcessor.ExtractIntegerProperty(jsonItem, "Armour");
            var evasion = _propertyProcessor.ExtractIntegerProperty(jsonItem, "Evasion");
            var energyShield = _propertyProcessor.ExtractIntegerProperty(jsonItem, "Energy Shield");
            var chanceToBlock = _propertyProcessor.ExtractIntegerProperty(jsonItem, "Chance to Block");

            armor.Armour = armour;
            armor.Evasion = evasion;
            armor.EnergyShield = energyShield;
            armor.ChanceToBlock = chanceToBlock;

            ComputeAdditionalProperties(armor);

            return armor;
        }

        private Item ComputeAdditionalProperties(Item armor)
        {
            if (armor.Quality == 20)
            {
                armor.ArmourWithMaxQuality = armor.Armour;
                armor.EvasionWithMaxQuality = armor.Evasion;
                armor.EnergyShieldWithMaxQuality = armor.EnergyShield;
            }
            else
            {
                var increasedArmour = armor.ExplicitMods.Where(e =>
                    e.Name == "X% increased Armour"
                    || e.Name == "X% increased Armour and Evasion"
                    || e.Name == "X% increased Armour and Energy Shield").FirstOrDefault()?.Value ?? 0.0M;

                var increasedEvasion = armor.ExplicitMods.Where(e =>
                    e.Name == "X% increased Evasion"
                    || e.Name == "X% increased Armour and Evasion"
                    || e.Name == "X% increased Evasion and Energy Shield").FirstOrDefault()?.Value ?? 0.0M;

                var increasedEnergyShield = armor.ExplicitMods.Where(e =>
                    e.Name == "X% increased Energy Shield"
                    || e.Name == "X% increased Armour and Energy Shield"
                    || e.Name == "X% increased Evasion and Energy Shield").FirstOrDefault()?.Value ?? 0.0M;

                var flatArmour = armor.Armour / (1 + ((increasedArmour - armor.Quality) / 100));
                var flatEvasion = armor.Evasion / (1 + ((increasedEvasion - armor.Quality) / 100));
                var flatEnergyShield = armor.EnergyShield / (1 + ((increasedEnergyShield - armor.Quality) / 100));

                armor.ArmourWithMaxQuality = (int)(flatArmour * (1 + (increasedArmour + 20) / 100));
                armor.EvasionWithMaxQuality = (int)(flatEvasion * (1 + (increasedEvasion + 20) / 100));
                armor.EnergyShieldWithMaxQuality = (int)(flatEnergyShield * (1 + (increasedEnergyShield + 20) / 100));
            }

            return armor;
        }
    }
}
