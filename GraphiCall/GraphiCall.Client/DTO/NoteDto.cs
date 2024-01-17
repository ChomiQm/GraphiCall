namespace GraphiCall.Client.DTO
{
    public class NoteDto
    {
        public string NoteId { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public NotePriority Priority { get; set; } 

        // Relacja z ApplicationUser
        public string ApplicationUserId { get; set; } = null!;

    }

    public enum NotePriority
    {
        High,
        Medium,
        Low
    }
}
