namespace Obligatorio_N3D_342742_360021_Client.Models
{
    public class LoginResponseDto
    {
        public string Token { get; private set; }
        public LoggedUserDTO User { get; private set; }

        private LoginResponseDto() { }
        public LoginResponseDto(string token, LoggedUserDTO user)
        {
            Token = token;
            User = user;
        }

    }
}