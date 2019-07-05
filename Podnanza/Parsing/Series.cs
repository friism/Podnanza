using System.Collections.Generic;

namespace Podnanza.Parsing
{
    public class Series
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public IEnumerable<Episode> Episodes { get; set; }
        public string Url { get; set; }
    }
}
