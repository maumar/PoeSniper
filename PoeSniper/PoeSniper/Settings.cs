using System.Collections.Generic;

namespace PoeSniper
{
    public class Settings
    {
        public string startingId { get; set; }
        public int sleepTreshold { get; set; }
        public int sleepTime { get; set; }
        public List<string> leagues { get; set; }
    }
}
