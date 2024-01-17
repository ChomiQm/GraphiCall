using GraphiCall.Client.DTO;
using GraphiCall.Client.Interfaces;
using GraphiCall.Data;
using GraphiCall.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("messages")]
public class MessagesController : ControllerBase
{
    private readonly ApplicationDbContext _chatContext;
    private readonly IHubContext<ChatHub, IChatHubClient> _hubContext;

    public MessagesController(ApplicationDbContext chatContext, IHubContext<ChatHub, IChatHubClient> hubContext)
    {
        _chatContext = chatContext;
        _hubContext = hubContext;
    }

    [HttpPost("send/{userId}/message")]
    public async Task<IActionResult> SendMessage([FromRoute] string userId, MessageSendDto messageDto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(messageDto.Message))
            return BadRequest();

        var message = new Message
        {
            FromId = userId,
            ToId = messageDto.ToUserId,
            Content = messageDto.Message,
            SentOn = DateTime.Now
        };
        await _chatContext.Messages.AddAsync(message, cancellationToken);
        if (await _chatContext.SaveChangesAsync(cancellationToken) > 0)
        {
            var responseMessageDto = new MessageDto(message.ToId, message.FromId, message.Content, message.SentOn);
            await _hubContext.Clients.User(messageDto.ToUserId.ToString())
                        .MessageRecieved(responseMessageDto);
            return Ok();
        }
        else
        {
            return StatusCode(500, "Unable to send message");
        }
    }

    [HttpGet("{userId}/between/{otherUserId}/messages")]
    public async Task<IEnumerable<MessageDto>> GetMessages([FromRoute] string userId, string otherUserId, CancellationToken cancellationToken)
    {
        var messages = await _chatContext.Messages
                        .AsNoTracking()
                        .Where(m =>
                            (m.FromId == otherUserId && m.ToId == userId)
                            || (m.ToId == otherUserId && m.FromId == userId)
                        )
                        .Select(m => new MessageDto(m.ToId, m.FromId, m.Content, m.SentOn))
                        .ToListAsync(cancellationToken);

        return messages;
    }
}