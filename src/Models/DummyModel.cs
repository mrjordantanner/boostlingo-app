using System.Text.Json.Serialization;

namespace boostlingo.models
{
    public class DummyModel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("language")]
        public string Language { get; set; } = string.Empty;

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("bio")]
        public string Bio {  get; set; } = string.Empty;

        [JsonPropertyName("version")]
        public double Version { get; set; }
    }
}
