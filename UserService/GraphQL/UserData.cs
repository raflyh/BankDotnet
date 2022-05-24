using Database.Models;

namespace UserService.GraphQL
{
    public partial class UserData
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public string Address { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public virtual ICollection<Balance> Balances { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
