using System.Text.Json.Serialization;

namespace GraphiCall.Data
{
    public class Note
    {
        public string NoteId { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public NotePriority Priority { get; set; } // Priorytet notatki, np. wysoki, średni, niski

        // Relacja z ApplicationUser
        public string ApplicationUserId { get; set; } = null!;
        [JsonIgnore]
        public virtual ApplicationUser? ApplicationUser { get; set; }
    }

    public enum NotePriority
    {
        High,
        Medium,
        Low
    }
}
