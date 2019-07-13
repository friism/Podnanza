using System;
using System.Collections.Generic;

namespace Podnanza.Parsing
{
    public class Episode
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public Uri WebUri { get; set; }
        public Uri AudioUri { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime Published { get; set; }
        public IEnumerable<string> Authors { get; internal set; }
        public long AudioFileLength { get; internal set; }
    }
}
