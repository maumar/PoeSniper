using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace PoeSniper
{
    public class Indexer
    {
        private ItemProcessor _itemProcessor;
        private NamesManager _namesManager;

        public void Start(string chunkId = null)
        {
            var settings = GetSettings();
            var _namesManager = new NamesManager();
            _namesManager.LoadNames();

            _itemProcessor = new ItemProcessor(settings.leagues, _namesManager);

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
            return null;
        }

        private string Index(string chunkId)
        {
            var jsonStashes = GetJsonStashes(chunkId);

            _itemProcessor.ProcessItems(jsonStashes);

            return jsonStashes.next_change_id;
        }

        private JsonStashes GetJsonStashes(string chunkId)
        {
            var apiUrl = "http://www.pathofexile.com/api/public-stash-tabs/" + chunkId;

            Console.WriteLine(DateTime.Now + " Fetching " +
                (string.IsNullOrEmpty(chunkId)
                    ? "first chunk"
                    : "chunk with ID: " + chunkId) + " ");

            var handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            var httpClient = new HttpClient(handler);
            var getStreamTask = httpClient.GetStreamAsync(apiUrl);
            getStreamTask.Wait();
            var stream = getStreamTask.Result;

            string value = string.Empty;
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                value = reader.ReadToEnd();
            }

            var rootObject = JsonConvert.DeserializeObject<JsonStashes>(value);

            return rootObject;
        }
    }
}
