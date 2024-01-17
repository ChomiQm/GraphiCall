namespace GraphiCall.Client.DTO
{
    public class CalendarDto
    {
        public string CalendarId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public List<CalendarEventDto>? Events { get; set; }

        // Relacja 1:1 z ApplicationUser
        public string ApplicationUserId { get; set; } = null!;
    }

    public class CalendarEventDto
    {
        public string CalendarEventId { get; set; } = null!;
        public string FK_CalendarId { get; set; } = null!;
        public string Title { get; set; } = null!;
        public DateTime EventDate { get; set; } = new DateTime(1990, 1, 1);
        public DateTime FromDate { get; set; } = new DateTime(1990, 1, 1);
        public DateTime ToDate { get; set; } = new DateTime(1990, 1, 1);
        public string? DateValue { get; set; }
        public string? DayName { get; set; }
        public string Color { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}
