namespace GraphiCall.Client.DTO
{
    public class WhiteboardDto
    {
        public string WhiteboardId { get; set; } = null!;
        public string? Data { get; set; } // JSON data
        public string ApplicationUserId { get; set; } = null!;
    }
}
