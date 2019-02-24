using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Podnanza.Parsing
{
    public class Bonanza
    {
        private readonly HttpClient _httpClient;

        private static readonly string itemUrlXPath = "(//div[contains(@class, 'list-content')])[1]//div[contains(@class, 'items')]//div[contains(@class, 'item')]/a";
        private static readonly string titleXPath = "(//div[contains(@class, 'list-title')])[1]//h2";
        private static readonly string audioXPath = "//audio/source";
        private static readonly string dataXPathFormat = "(//div[@class='row' and div[text()='{0}:']])[1]/div[contains(@class, 'asset-body')]/p";
        private static readonly string canonicalUrlXPath = "//head/link[@rel='canonical']";

        public Bonanza(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Series> GetSeries(int id)
        {
            var response = await _httpClient.GetAsync($"https://www.dr.dk/bonanza/serie/{id}/x");
            var pageContents = await response.Content.ReadAsStringAsync();
            var document = new HtmlDocument();
            document.LoadHtml(pageContents);
            var node = document.DocumentNode;

            return new Series
            {
                Title = node.SelectSingleNode(titleXPath).InnerText,
                Url = node.SelectSingleNode(canonicalUrlXPath).Attributes["href"].Value,
                Episodes = await GetEpisodes(node)
            };
        }

        private async Task<IEnumerable<Episode>> GetEpisodes(HtmlNode node)
        {
            var itemLinks = node.SelectNodes(itemUrlXPath);
            var tasks = itemLinks.Select(
                i => GetEpisode(string.Format("https://www.dr.dk{0}", i.Attributes["href"].Value)));
            return await Task.WhenAll(tasks);
        }

        private async Task<Episode> GetEpisode(string url)
        {
            var response = await _httpClient.GetAsync(url);
            var pageContents = await response.Content.ReadAsStringAsync();
            var document = new HtmlDocument();
            document.LoadHtml(pageContents);
            var node = document.DocumentNode;

            var published = DateTime.Parse(GetEpisodeInfo(node, "Sendt"),
                CultureInfo.CreateSpecificCulture("da-DK"));

            var audioUri =
                (new UriBuilder(node.SelectSingleNode(audioXPath).Attributes["src"].Value) {
                    Scheme = "https",
                    Port = -1
                })
                .Uri;

            return new Episode
            {
                WebUri = new Uri(url),
                AudioUri = audioUri,
                AudioFileLength = await GetAudioFileLength(audioUri),
                Description = HtmlEntity.DeEntitize(GetEpisodeInfo(node, "Programinfo"))
                    .Replace("\t", "").Replace("\n", "").Replace("\r",""),
                Title = HtmlEntity.DeEntitize(node.SelectSingleNode("//h1").InnerText),
                Published = published,
                Duration = TimeSpan.Parse(GetEpisodeInfo(node, "Tid")),
                Author = GetEpisodeInfo(node, "Medvirkende").TrimEnd(new char[] { ';', '.' }),
            };
        }

        private string GetEpisodeInfo(HtmlNode node, string label)
        {
            return node.SelectSingleNode(string.Format(dataXPathFormat, label)).InnerText;
        }

        private async Task<long> GetAudioFileLength(Uri uri)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Head, uri))
            using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            {
                return long.Parse(
                    response.Content.Headers.Single(x => x.Key.Equals("content-length", StringComparison.InvariantCultureIgnoreCase))
                    .Value.Single());
            }
        }
    }
}
