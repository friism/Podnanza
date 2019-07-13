using Microsoft.AspNetCore.Mvc;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Rss;
using Podnanza.Parsing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Podnanza.Controllers
{
    [Route("p")]
    public class PodcastController : Controller
    {
        private readonly HttpClient _httpClient;
        private const string _itunesNs = "http://www.itunes.com/dtds/podcast-1.0.dtd";
        private readonly CultureInfo _daCulture = CultureInfo.CreateSpecificCulture("da-DK");

        public PodcastController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet("{id}")]
        [HttpHead("{id}")]
        public async Task Get(int id)
        {
            try
            {
                var series = await new Bonanza(_httpClient).GetSeries(id);
                Response.ContentType = "application/rss+xml";

                using (var xmlWriter = XmlWriter.Create(Response.Body, new XmlWriterSettings()
                {
                    Async = true,
                    Encoding = Encoding.UTF8
                }))
                {
                    var attributes = new List<SyndicationAttribute>()
                {
                    new SyndicationAttribute("xmlns:itunes", _itunesNs),
                };
                    var formatter = new RssFormatter(attributes, xmlWriter.Settings);
                    var feedWriter = new RssFeedWriter(xmlWriter, attributes, formatter);

                    await WriteFeedPreamble(feedWriter);

                    await feedWriter.WriteTitle(series.Title);
                    await feedWriter.Write(new SyndicationContent("subtitle", _itunesNs,
                        $"{series.Title} fra DR Bonanza, podcastet af Podnanza"));
                    await feedWriter.Write(new SyndicationLink(new Uri(series.Url)));
                    await feedWriter.Write(new SyndicationContent("author", _itunesNs, series.Author));
                    await feedWriter.Write(new SyndicationContent("summary", _itunesNs, series.Description));

                    foreach (var x in series.Episodes.OrderBy(x => x.Published)
                        .Select((value, i) => new { i, value }))
                    {
                        var episode = x.value;

                        var item = new SyndicationItem
                        {
                            Title = episode.Title,
                            Description = episode.Description,
                            Published = episode.Published,
                            Id = $"{episode.WebUri}",
                        };

                        item.AddLink(new SyndicationLink(episode.AudioUri, RssLinkTypes.Enclosure)
                        {
                            MediaType = "audio/mpeg",
                            Length = episode.AudioFileLength,
                        });
                        item.AddLink(new SyndicationLink(episode.WebUri));

                        var content = new SyndicationContent(formatter.CreateContent(item));
                        content.AddField(new SyndicationContent("summary", _itunesNs, episode.Description));
                        content.AddField(new SyndicationContent("author", _itunesNs, string.Join(", ", episode.Authors)));
                        content.AddField(new SyndicationContent("duration", _itunesNs,
                            episode.Duration.ToString(@"hh\:mm\:ss")));
                        content.AddField(new SyndicationContent("explicit", _itunesNs, "no"));
                        content.AddField(new SyndicationContent("episode", _itunesNs, $"{x.i + 1}"));
                        content.AddField(new SyndicationContent("language", _itunesNs, $"{_daCulture}"));
                        await feedWriter.Write(content);
                    }
                }
            }
            catch (SeriesNotFoundException)
            {
                Response.StatusCode = 404;
                return;
            }
        }

        private async Task WriteFeedPreamble(RssFeedWriter feedWriter)
        {
            await feedWriter.WriteCopyright("DR");
            await feedWriter.WriteGenerator("Podnanza");
            await feedWriter.Write(GetOwner());
            await feedWriter.Write(GetLogo());
            await feedWriter.Write(GetCategory());
            await feedWriter.WriteLanguage(_daCulture);
            await feedWriter.Write(new SyndicationContent("language", _itunesNs, $"{_daCulture}"));
            await feedWriter.Write(new SyndicationContent("explicit", _itunesNs, "no"));
            await feedWriter.Write(new SyndicationContent("complete", _itunesNs, "Yes"));
            await feedWriter.Write(new SyndicationContent("type", _itunesNs, "serial"));
        }

        private ISyndicationContent GetLogo()
        {
            var logoContent = new SyndicationContent("image", _itunesNs, null);
            logoContent.AddAttribute(new SyndicationAttribute("href", "https://p.friism.com/podnanza.jpg"));

            return logoContent;
        }

        private ISyndicationContent GetCategory()
        {
            var logoContent = new SyndicationContent("category", _itunesNs, null);
            logoContent.AddAttribute(new SyndicationAttribute("text", "Kids & Family"));

            return logoContent;
        }

        private ISyndicationContent GetOwner()
        {
            var ownerContent = new SyndicationContent("owner", _itunesNs, null);
            ownerContent.AddField(new SyndicationContent("name", _itunesNs, "Michael Friis"));
            ownerContent.AddField(new SyndicationContent("email", _itunesNs, "friism@gmail.com"));

            return ownerContent;
        }
    }
}
