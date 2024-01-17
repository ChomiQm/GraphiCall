namespace GraphiCall.Client.DTO
{
    public record MessageDto(string ToUserId, string FromUserId, string Message, DateTime SentOn);
}
