using System;
using System.Collections.Generic;

namespace Podnanza.Parsing
{
    public class Episode
    {
        public string Title { get; internal set; }
        public string Description { get; internal set; }
        public Uri WebUri { get; internal set; }
        public Uri MediaUri { get; internal set; }
        public TimeSpan Duration { get; internal set; }
        public DateTime Published { get; internal set; }
        public IEnumerable<string> Authors { get; internal set; }
        public long FileLength { get; internal set; }
        public string MediaType { get; internal set; }
    }
}
