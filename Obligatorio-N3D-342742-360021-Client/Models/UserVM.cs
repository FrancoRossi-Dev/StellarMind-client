namespace Obligatorio_N3D_342742_360021_Client.Models
{
    public class UserVM
    {
        public int UserId { get; init; }
        public string fullName { get; init; }
        public string username { get; init; }
        public string phoneNumber { get; init; }
        public string email { get; init; }
        public string password { get; init; }
        public string address { get; init; }
        public string role { get; init; }

        private UserVM() { }

        public UserVM(int userId, string fullName, string username, string phoneNumber, string email, string password, string address, string role)
        {
            UserId = userId;
            this.fullName = fullName;
            this.username = username;
            this.phoneNumber = phoneNumber;
            this.email = email;
            this.password = password;
            this.address = address;
            this.role = role;
        }

        public UserVM(string fullName, string username, string phoneNumber, string email, string password, string address, string role)
            : this(0, fullName, username, phoneNumber, email, password, address, role) { }
    }
}
