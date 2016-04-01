using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PoeSniper
{
    public class PriceProcessor
    {
        private readonly Dictionary<string, Currency> _currencyNameMap = new Dictionary<string, Currency>
        {
            { "alch", Currency.Alchemy },
            { "alc", Currency.Alchemy },
            { "alt", Currency.Alteration },
            { "blessed", Currency.Blessed },
            { "chaos", Currency.Chaos },
            { "chisel", Currency.Chisel },
            { "chance", Currency.Chance },
            { "chrom", Currency.Chromatic },
            { "c", Currency.Chaos },
            { "divine", Currency.Divine },
            { "exa", Currency.Exalted },
            { "ex", Currency.Exalted },
            { "fuse", Currency.Fusing },
            { "fusing", Currency.Fusing },
            { "gcp", Currency.GCP },
            { "jew", Currency.Jeweller },
            { "mirror", Currency.Mirror },
            { "regal", Currency.Regal },
            { "regret", Currency.Regret },
            { "scour", Currency.Scour },
            { "transmutation", Currency.Transmutation },
            { "vaal", Currency.Vaal },
        };

        private Regex _priceRegex;

        public PriceProcessor()
        {
            var currencyTypePattern = string.Join("|", _currencyNameMap.Keys);
            _priceRegex = new Regex(@"(?<priceType>~(price|b/o|c/o))\s*(?<value>[\d./]+)\s*(?<currencyType>" + currencyTypePattern + ")", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        public ItemPrice ProcessPrice(string priceString)
        {
            if (string.IsNullOrEmpty(priceString))
            {
                return null;
            }

            var itemPrice = new ItemPrice
            {
                PriceString = priceString
            };

            var priceMatch = _priceRegex.Match(priceString);
            if (priceMatch.Success)
            {
                Currency currency;
                if (_currencyNameMap.TryGetValue(priceMatch.Groups["currencyType"].Value.ToLower(), out currency))
                {
                    itemPrice.Currency = currency;
                }
                else
                {
                    itemPrice.Currency = null;
                }

                itemPrice.Type = ParsePriceType(priceMatch.Groups["priceType"].Value);
                itemPrice.Value = ParsePriceValue(priceMatch.Groups["value"].Value);
            }

            return itemPrice;
        }

        private PriceType? ParsePriceType(string priceTypeMatch)
        {
            if (priceTypeMatch == "~price")
            {
                return PriceType.FixedPrice;
            }
            else if (priceTypeMatch == "~c/o")
            {
                return PriceType.CurrentOffer;
            }
            else if (priceTypeMatch == "~b/o")
            {
                return PriceType.Buyout;
            }

            return null;
        }

        private decimal? ParsePriceValue(string priceValueMatch)
        {
            decimal value = 0.0M;
            if (decimal.TryParse(priceValueMatch, out value))
            {
                return value;
            }

            var fraction = priceValueMatch.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
            if (fraction.Count() == 2)
            {
                decimal numerator;
                decimal denominator;
                if (decimal.TryParse(fraction[0], out numerator) && decimal.TryParse(fraction[1], out denominator))
                {
                    return numerator / denominator;
                }
            }

            return null;
        }
    }
}
