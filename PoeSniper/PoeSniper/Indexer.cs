using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace PoeSniper
{
    public class Indexer
    {
        private const string _itemApiUrl = @"http://www.pathofexile.com/api/public-stash-tabs/";
        private const string _exileToolsUrl = @"http://api.exiletools.com/stats/_search";

        private Logger _logger;
        private ItemProcessor _itemProcessor;
        private NamesManager _namesManager;

        public void Start(string chunkId = null)
        {
            var settings = GetSettings();
            _logger = new Logger(LogLevel.Information);

            var _namesManager = new NamesManager(_logger);
            _namesManager.Initialize();

            _itemProcessor = new ItemProcessor(settings, _namesManager, _logger);

            if (string.IsNullOrEmpty(chunkId))
            {
                chunkId = settings.startingId;
            }

            if (string.IsNullOrEmpty(chunkId))
            {
                chunkId = GetLatestChunkId();
            }

            if (string.IsNullOrEmpty(chunkId))
            {
                throw new InvalidOperationException("Couldn't find starting ID. Please specify it in settings.json");
            }

            while(true)
            {
                chunkId = Index(chunkId);
            }
        }

        private Settings GetSettings()
        {
            var settingsJson = File.ReadAllText("settings.json");
            var settings = JsonConvert.DeserializeObject<Settings>(settingsJson);

            return settings;
        }

        private string GetLatestChunkId()
        {
            var payload = @"{ ""query"": { ""match"": { ""_type"": ""run"" } },""sort"": [ { ""runTime"": { ""order"": ""desc"" }}], size:1 }";
            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                var response = client.UploadString(_exileToolsUrl, "POST", payload);

                var changeIdRegex = new Regex(@"\""change_id\"":\""(?<value>[\d-]+)\""", RegexOptions.Compiled);
                var match = changeIdRegex.Match(response);
                if (match.Success)
                {
                    return match.Groups["value"].Value;
                }
                else
                {
                    _logger.Error("Couldn't fetch change ID from ExileTools");
                    return null;
                }
            }
        }

        private string Index(string chunkId)
        {
            var jsonStashes = GetJsonStashes(chunkId);

            _itemProcessor.ProcessItems(jsonStashes);

            return jsonStashes.next_change_id;
        }

        private JsonStashes GetJsonStashes(string chunkId)
        {
            _logger.Information(DateTime.Now + " Fetching " + chunkId, newLine: false);

            var sw = new Stopwatch();
            sw.Start();
            var handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            var httpClient = new HttpClient(handler);
            var getStreamTask = httpClient.GetStreamAsync(_itemApiUrl + chunkId);
            getStreamTask.Wait();
            var stream = getStreamTask.Result;

            string value = string.Empty;
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                value = reader.ReadToEnd();
            }

            var rootObject = JsonConvert.DeserializeObject<JsonStashes>(value);

            _logger.Information(" | Done in " + sw.Elapsed);

            return rootObject;
        }
    }
}
