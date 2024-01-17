using System.Text.Json.Serialization;

namespace GraphiCall.Data
{
    public class Whiteboard
    {
        public string WhiteboardId { get; set; } = null!;
        public string? Data { get; set; } // JSON data NVARCHAR(MAX)
        public string ApplicationUserId { get; set; } = null!;
        [JsonIgnore]
        public virtual ApplicationUser? ApplicationUser { get; set; }
    }
}
