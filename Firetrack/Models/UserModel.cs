using SQLite;

namespace Firetrack.Models
{
    [Table("Users")]
    public class UserModel
    {
        [PrimaryKey, AutoIncrement]
        public int UserId { get; set; }

        [Unique, NotNull]
        public string Username { get; set; } = string.Empty;

        [NotNull]
        public string Password { get; set; } = string.Empty;

        [NotNull]
        public string FullName { get; set; } = string.Empty;

        [NotNull]
        public string Role { get; set; } = "Personnel";
    }
}