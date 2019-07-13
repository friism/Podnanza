using System.Collections.Generic;
using System.Linq;

namespace Podnanza.Parsing
{
    public class Series
    {
        public string Title { get; set; }
        public string Author
        {
            get
            {
                // if all episodes have the same author, use that
                var authors = Episodes.SelectMany(x => x.Authors).Distinct();
                if (authors.Count() == 1)
                {
                    return authors.Single();
                }
                else
                {
                    return string.Join(", ", authors);
                }
            }
        }
        public string Description
        {
            get
            {
                // if all episodes have the same description, use that
                if (Episodes.Select(x => x.Description).Distinct().Count() == 1)
                {
                    return Episodes.Single().Description;
                }
                else
                {
                    return Title;
                }
            }
        }
        public IEnumerable<Episode> Episodes { get; set; }
        public string Url { get; set; }
    }
}
