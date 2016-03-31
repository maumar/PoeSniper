using System;
using System.Linq;

namespace PoeSniper
{
    public class PropertyProcessor
    {
        private Logger _logger;

        public PropertyProcessor(Logger logger)
        {
            _logger = logger;
        }

        public int ExtractIntegerProperty(JsonItem jsonItem, string propertyName, int defaultValue = 0)
        {
            var propertyValue = defaultValue;
            if (jsonItem.properties != null)
            {
                var propertyString = (string)jsonItem.properties.Where(p => p.name == propertyName).FirstOrDefault()?.values?[0]?[0];
                if (propertyString != null)
                {
                    if (!int.TryParse(propertyString.Replace("%", ""), out propertyValue))
                    {
                        _logger.Error("Couldn't parse property. Name: '" + propertyName + "' Value: '" + propertyString + "'");
                    }
                }
            }

            return propertyValue;
        }

        public decimal ExtractDecimalProperty(JsonItem jsonItem, string propertyName, decimal defaultValue = 0.0M)
        {
            var propertyValue = defaultValue;
            if (jsonItem.properties != null)
            {
                var propertyString = (string)jsonItem.properties.Where(p => p.name == propertyName).FirstOrDefault()?.values?[0]?[0];
                if (propertyString != null)
                {
                    if (!decimal.TryParse(propertyString.Replace("%", ""), out propertyValue))
                    {
                        _logger.Error("Couldn't parse property. Name: '" + propertyName + "' Value: '" + propertyString + "'");
                    }
                }
            }

            return propertyValue;
        }

        public decimal ExtractRangeProperty(JsonItem jsonItem, string propertyName, decimal defaultValue = 0.0M)
        {
            var propertyValue = defaultValue;
            if (jsonItem.properties != null)
            {
                var propertyString = (string)jsonItem.properties.Where(p => p.name == propertyName).FirstOrDefault()?.values?[0]?[0];
                if (propertyString != null)
                {
                    propertyValue = ExtractRangePropertyValue(propertyString, propertyName);
                }
            }

            return propertyValue;
        }

        public decimal ExtractRangePropertyValue(string propertyString, string propertyName)
        {
            var rangeValues = propertyString.Split(new[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
            if (rangeValues.Count() != 2)
            {
                _logger.Error("Couldn't parse range property. Name: '" + propertyName + "' Value: '" + propertyString + "'");
            }

            int minValue;
            if (!int.TryParse(rangeValues[0], out minValue))
            {
                _logger.Error("Couldn't parse min value of a range property. Name: '" + propertyName + "' Value: '" + rangeValues[0] + "'");
            }

            int maxValue;
            if (!int.TryParse(rangeValues[1], out maxValue))
            {
                _logger.Error("Couldn't parse max value of a range property. Name: '" + propertyName + "' Value: '" + rangeValues[1] + "'");
            }

            return (maxValue + minValue) / 2.0M;
        }
    }
}
