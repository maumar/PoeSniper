using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Item
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Base { get; set; }

        public bool IsIdentified { get; set; }
        public bool IsCorrupted { get; set; }

        public int Quality { get; set; }
        //public ItemRequirements Requirements { get; set; }
        //public List<ItemProperty> ImplicitProperties { get; set; }
        //public List<ItemProperty> ExplicitProperties { get; set; }
        //public List<GemSocket> Sockets { get; set; }

        public string League { get; set; }
        public StashTab StashTab { get; set; }

        //public Currency? PriceCurrency { get; set; }
        public decimal? PriceAmount { get; set; }

        //public DateTime PostDate { get; set; }
    }
}
