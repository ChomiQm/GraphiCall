namespace GraphiCall.Client.DTO
{
    public class UserDto
    {
        public UserDto(string id, string email, bool isOnline = false)
        {
            Id = id;
            Email = email;
            IsOnline = isOnline;
        }

        public string Id { get; set; }
        public string Email { get; set; }
        public bool IsOnline { get; set; }
        public bool IsSelected { get; set; }
    }
}
