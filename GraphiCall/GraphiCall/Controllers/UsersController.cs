using GraphiCall.Client.DTO;
using GraphiCall.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GraphiCall.Controllers
{
    [ApiController]
    [Route("users")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }        

        [HttpGet("getUsers/{userId}")]
        public async Task<IEnumerable<UserDto>> GetUsers([FromRoute] string userId)
        {
            return await _context.Users
                        .AsNoTracking()
                        .Where(u => u.Id != userId)
                        .Select(u => new UserDto(u.Id, u.Email, false))
                        .ToListAsync();
        }

        [HttpGet("getChats/{userId}")]
        public async Task<IEnumerable<UserDto>> GetUserChats([FromRoute] string userId, CancellationToken cancellationToken)
        {
            IEnumerable<UserDto> chatUsers = new List<UserDto>();
            var uniqueUsers = await _context.Messages
                        .AsNoTracking()
                        .Where(m => m.FromId == userId || m.ToId == userId)
                        .Select(m => new { From = m.FromId, To = m.ToId })
                        .Distinct()
                        .ToListAsync(cancellationToken);

            var uniqueUserIds = new HashSet<string>();
            uniqueUsers.ForEach(u =>
            {
                if (u.From != userId)
                    uniqueUserIds.Add(u.From);
                if (u.To != userId)
                    uniqueUserIds.Add(u.To);
            });
            if (uniqueUserIds.Count > 0)
            {
                chatUsers = await _context.Users
                                    .AsNoTracking()
                                    .Where(u => uniqueUserIds.Contains(u.Id))
                                    .Select(u => new UserDto(u.Id, u.Email, false))
                                    .ToListAsync(cancellationToken);
            }

            return chatUsers;
        }

    }
    
}
