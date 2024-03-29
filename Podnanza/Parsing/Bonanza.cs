﻿using HtmlAgilityPack;
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
        private static readonly string mediaXPath = "//source";
        private static readonly string dataXPathFormat = "(//div[@class='row' and div[normalize-space()='{0}:']])[1]/div[contains(@class, 'asset-body')]/p";
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

            // TODO: Find better way to handle the invalid episodes than returning null
            var episodes = (await GetEpisodes(node)).Where(x => x != null);

            var result = new Series
            {
                Title = node.SelectSingleNode(titleXPath).InnerText.Replace("Episode:", "").Trim(),
                Url = node.SelectSingleNode(canonicalUrlXPath).Attributes["href"].Value,
                Episodes = episodes,
            };

            return result;
        }

        private async Task<IEnumerable<Episode>> GetEpisodes(HtmlNode node)
        {
            var itemLinks = node.SelectNodes(itemUrlXPath);
            if (itemLinks is null)
            {
                throw new SeriesNotFoundException();
            }

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

            var mediaNode = node.SelectSingleNode(mediaXPath);
            var mediaUri =
                (new UriBuilder(mediaNode.Attributes["src"].Value) {
                    Scheme = "https",
                    Port = -1
                })
                .Uri;
            var mediaType = mediaNode.Attributes["type"].Value;

            if (mediaType.Equals("application/x-mpegURL", StringComparison.OrdinalIgnoreCase))
            {
                // This is a HLS-stream-formatted video-podcast like here: https://www.dr.dk/bonanza/serie/302/montage/72403/nattevaegteren---en-gyser-uegnet-for-mindre-boern---the-night-watchman
                // It does not have a file length, and the Podcast app doesn't know what to do with these streams
                return null;
            }

            return new Episode
            {
                WebUri = new Uri(url),
                MediaUri = mediaUri,
                MediaType = mediaType,
                FileLength = await GetFileLength(mediaUri),
                Description = HtmlEntity.DeEntitize(GetEpisodeInfo(node, "Programinfo"))
                    .Replace("\t", "").Replace("\n", "").Replace("\r","")
                    .Replace("B&U - BørneRadio - P3 - ", ""),
                Title = HtmlEntity.DeEntitize(node.SelectSingleNode("//h1").InnerText)
                    .Replace("Børneradio: ", "")
                    .Replace("Børneradio - ", "")
                    .Replace("Radioteatret: ", ""),
                Published = published,
                Duration = TimeSpan.Parse(GetEpisodeInfo(node, "Tid")),
                Authors = GetAuthors(node),
            };
        }

        private IEnumerable<string> GetAuthors(HtmlNode node)
        {
            var authorValueString = GetEpisodeInfo(node, "Medvirkende")?.TrimEnd(new char[] { ';', '.' });
            if(string.IsNullOrWhiteSpace(authorValueString))
            {
                yield break;
            }

            var authors = authorValueString.Split(new string[] { ";", " og " }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x));
            foreach (var author in authors)
            {
                yield return author;
            }
        }

        private string GetEpisodeInfo(HtmlNode node, string label)
        {
            return node.SelectSingleNode(string.Format(dataXPathFormat, label))?.InnerText ?? null;
        }

        private async Task<long> GetFileLength(Uri uri)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Head, uri))
            using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            {
                return response.Content.Headers.ContentLength.Value;
            }
        }
    }
}
