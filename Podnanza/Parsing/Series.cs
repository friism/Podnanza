using System.Collections.Generic;
using System.Linq;

namespace Podnanza.Parsing
{
    public class Series
    {
        public string Title { get; set; }
        public string SubTitle { get
            {
                return $"{Title} fra DR Bonanza, podcastet af Podnanza";
            }
        }
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
                var descriptions = Episodes.Select(x => x.Description).Distinct();
                if (descriptions.Count() == 1)
                {
                    return descriptions.Single();
                }
                else
                {
                    return $"En serie af radioprogrammer med titlen \"{Title}\" fra DR Bonanza arkivet. Podcasten er automatisk genereret af Podnanza.";
                }
            }
        }
        public IEnumerable<Episode> Episodes { get; set; }
        public string Url { get; set; }
    }
}
