namespace MoneyPlannerWebAPI.DTO.UserDto
{
    public class PostUserDto
    {
        public PostUserDto(string username, string password)
        {
            Username = username;
            Password = password;
        }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
