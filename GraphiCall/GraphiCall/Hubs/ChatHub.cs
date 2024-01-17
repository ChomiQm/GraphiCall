using GraphiCall.Client.DTO;
using GraphiCall.Client.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace GraphiCall.Hubs
{
    public class ChatHub : Hub<IChatHubClient>, IChatHubServer
    {
        private static readonly IDictionary<string, UserDto> _onlineUsers = new Dictionary<string, UserDto>();

        public ChatHub() { }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public async Task SetUserOnline(UserDto user)
        {
            await Clients.Caller.OnlineUsersList(_onlineUsers.Values);
            if (!_onlineUsers.ContainsKey(user.Id))
            {
                _onlineUsers.Add(user.Id, user);
                await Clients.Others.UserIsOnline(user.Id);
            }
        }
    }
}
