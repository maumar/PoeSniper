using System.Linq;
using Model;

namespace PoeSniper
{
    public class WeaponProcessor
    {
        private PropertyProcessor _propertyProcessor;
        private NamesManager _namesManager;
        private Logger _logger;

        public WeaponProcessor(PropertyProcessor propertyProcessor, NamesManager namesManager, Logger logger)
        {
            _propertyProcessor = propertyProcessor;
            _namesManager = namesManager;
            _logger = logger;
        }

        public Item ProcessWeapon(Item weapon, JsonItem jsonItem)
        {
            var weaponType = (string)jsonItem.properties.FirstOrDefault().name;
            if (string.IsNullOrEmpty(weaponType))
            {
                _logger.Error("Weapon type not found. Item name: " + weapon.Name);
            }

            _namesManager.VerifyWeaponType(weaponType);

            var attacksPerSecond = _propertyProcessor.ExtractDecimalProperty(jsonItem, "Attacks per Second");
            var physicalDamage = _propertyProcessor.ExtractRangeProperty(jsonItem, "Physical Damage");
            var chaosDamage = _propertyProcessor.ExtractRangeProperty(jsonItem, "Chaos Damage");
            var criticalStrikeChance = _propertyProcessor.ExtractDecimalProperty(jsonItem, "Critical Strike Chance");

            weapon.WeaponType = weaponType;
            weapon.AttacksPerSecond = attacksPerSecond;
            weapon.PhysicalDamage = physicalDamage;
            weapon.ChaosDamage = chaosDamage;
            weapon.CriticalStrikeChance = criticalStrikeChance;

            ProcessElementalDamage(weapon, jsonItem);
            ComputeAdditionalProperties(weapon);

            return weapon;
        }

        private Item ProcessElementalDamage(Item weapon, JsonItem jsonItem)
        {
            var elementalDamageProperty = jsonItem.properties.Where(p => p.name == "Elemental Damage").FirstOrDefault();
            if (elementalDamageProperty != null)
            {
                foreach (var value in elementalDamageProperty.values)
                {
                    var damageValue = _propertyProcessor.ExtractRangePropertyValue((string)value[0], "Elemental Damage");
                    switch ((long)value[1])
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
                            _logger.Error("Invalid Elemental Damage type: " + value[1]);
                            break;
                    }
                }
            }

            return weapon;
        }

        private Item ComputeAdditionalProperties(Item weapon)
        {
            if (weapon.Quality == 20)
            {
                weapon.PhysicalDpsWithMaxQuality = weapon.PhysicalDps;
            }
            else
            {
                weapon.PhysicalDps = weapon.PhysicalDamage * weapon.AttacksPerSecond;
                weapon.ElementalDps = (weapon.FireDamage + weapon.ColdDamage + weapon.LightningDamage + weapon.ChaosDamage) * weapon.AttacksPerSecond;
                weapon.Dps = weapon.PhysicalDps + weapon.ElementalDps;

                var increasedPhysicalDamage = weapon.ExplicitMods.Where(m => m.Name == "X% increased Physical Damage").FirstOrDefault()?.Value ?? 0.0M;

                var flatPhysicalDamage = weapon.PhysicalDamage / (1.0M + ((increasedPhysicalDamage + weapon.Quality) / 100));
                weapon.PhysicalDpsWithMaxQuality = flatPhysicalDamage * (1.2M + (increasedPhysicalDamage / 100)) * weapon.AttacksPerSecond;
            }

            weapon.DpsWithMaxQuality = weapon.PhysicalDpsWithMaxQuality + weapon.ElementalDps;

            return weapon;
        }
    }
}
