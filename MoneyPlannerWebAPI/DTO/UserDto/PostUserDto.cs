namespace MoneyPlannerWebAPI.DTO.UserDto
{
    public class PostUserDto
    {
        public PostUserDto(string username)
        {
            Username = username;
        }
        public string Username { get; set; }
    }
}
