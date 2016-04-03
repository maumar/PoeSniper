using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace PoeSniper
{
    public class PriceProcessor
    {
        private const string _currencyExchangeRatesFileName = @"data/currencyExchangeRates.json";

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
        private Logger _logger;

        private Dictionary<Currency, decimal> _currencyExchangeRates;
        public PriceProcessor(Logger logger)
        {
            _logger = logger;
            var currencyTypePattern = string.Join("|", _currencyNameMap.Keys);
            _priceRegex = new Regex(@"(?<priceType>~(price|b/o|c/o))\s*(?<value>[\d./]+)\s*(?<currencyType>" + currencyTypePattern + ")", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            LoadCurrencyExchangeRates();
        }

        private void LoadCurrencyExchangeRates()
        {
            _currencyExchangeRates = new Dictionary<Currency, decimal>();

            var currencyExchangeRatesString = File.ReadAllText(_currencyExchangeRatesFileName);
            var currencyExchangeRatesJson = JsonConvert.DeserializeObject<JsonExchangeRates>(currencyExchangeRatesString);

            foreach (var currencyExchangeRate in currencyExchangeRatesJson.exchangeRates)
            {
                var exchangeDecimal = ParseDecimalOrFraction(currencyExchangeRate.Value);
                if (exchangeDecimal != null)
                {
                    _currencyExchangeRates.Add(currencyExchangeRate.Key, exchangeDecimal.Value);
                }
                else
                {
                    _logger.Error("Invalid currency exchange rate for " + currencyExchangeRate.Key + " Exchange string: ')" + currencyExchangeRate.Value + "'");
                }
            }

            _currencyExchangeRates.Add(Currency.Chaos, 1);
        }

        public ItemPrice ProcessPrice(string priceString)
        {
            if (string.IsNullOrEmpty(priceString))
            {
                return null;
            }

            var priceMatch = _priceRegex.Match(priceString);
            if (priceMatch.Success)
            {
                var priceType = ParsePriceType(priceMatch.Groups["priceType"].Value);
                var value = ParseDecimalOrFraction(priceMatch.Groups["value"].Value);
                var currencyType = ParseCurrencyType(priceMatch.Groups["currencyType"].Value.ToLower());
                if (priceType != null && value != null && currencyType != null)
                {
                    return new ItemPrice { Type = priceType.Value, Value = value.Value, Currency = currencyType.Value };
                }
            }

            return null;
        }

        public decimal? CalculateExchangeRate(decimal fromAmount, Currency fromCurrency, Currency toCurrency = Currency.Chaos)
        {
            decimal fromRatio;
            if (!_currencyExchangeRates.TryGetValue(fromCurrency, out fromRatio))
            {
                return null;
            }

            decimal toRatio;
            if (!_currencyExchangeRates.TryGetValue(toCurrency, out toRatio))
            {
                return null;
            }

            return Math.Round(fromAmount * fromRatio / toRatio, 2);
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

        private decimal? ParseDecimalOrFraction(string decimalOrFractionString)
        {
            decimal value = 0.0M;
            if (decimal.TryParse(decimalOrFractionString, out value))
            {
                return value;
            }

            var fraction = decimalOrFractionString.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
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

        private Currency? ParseCurrencyType(string currencyTypeMatch)
        {
            Currency currency;
            if (_currencyNameMap.TryGetValue(currencyTypeMatch, out currency))
            {
                return currency;
            }
            else
            {
                return null;
            }
        }
    }
}
