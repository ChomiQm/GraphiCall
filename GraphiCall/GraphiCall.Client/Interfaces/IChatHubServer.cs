using GraphiCall.Client.DTO;

namespace GraphiCall.Client.Interfaces
{
    public interface IChatHubServer
    {
        Task SetUserOnline(UserDto user);
    }
}
