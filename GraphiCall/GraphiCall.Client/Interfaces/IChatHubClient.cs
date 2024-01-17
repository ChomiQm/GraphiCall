using GraphiCall.Client.DTO;

namespace GraphiCall.Client.Interfaces
{
    public interface IChatHubClient
    {
        Task UserConnected(UserDto user);
        Task OnlineUsersList(IEnumerable<UserDto> users);
        Task UserIsOnline(string userId);
        Task MessageRecieved(MessageDto message);
    }
}
