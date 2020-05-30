using System.Collections.Generic;
using Newtonsoft.Json;

namespace CouchDB.Driver.DTOs
{
    public class ChangesFeedResponse
    {
        [JsonProperty("last_seq")]
        public string LastSequence { get; set; }

        [JsonProperty("pending")]
        public int Pending { get; set; }

        [JsonProperty("results")]
        public IList<ChangesFeedResponseResult> Results { get; internal set; }
    }

    public class ChangesFeedResponseResult
    {
        [JsonProperty("changes")]
        public IList<ChangesFeedResponseResultChange> Changes { get; internal set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("seq")]
        public string Seq { get; set; }

        [JsonProperty("deleted")]
        public bool Deleted { get; set; }
    }

    public class ChangesFeedResponseResultChange
    {
        [JsonProperty("rev")]
        public string Rev { get; set; }
    }
}
