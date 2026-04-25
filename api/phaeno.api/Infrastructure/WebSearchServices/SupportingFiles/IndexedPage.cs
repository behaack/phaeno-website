using System.Text.Json.Serialization;

namespace phaeno.api.Infrastructure.WebSearchServices.SupportingFiles
{
    public class IndexedPage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Url { get; set; } = "";
        public string PageTitle { get; set; } = "";
        public string Anchor { get; set; } = "";
        public string AnchorTitle { get; set; } = "";
        public string Description { get; set; } = "";
        public string DocumentType { get; set; } = "";
        public string? Snippet { get; set; } = "";
        public float? Score { get; set; }
        public int? Count { get; set; }
        [JsonIgnore]
        public string? Text { get; set; } = "";
        [JsonIgnore]
        public DateTime IndexedAt { get; set; } = DateTime.UtcNow;
    }
}
