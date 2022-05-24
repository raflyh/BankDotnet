using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class User
    {
        public User()
        {
            Balances = new HashSet<Balance>();
            Credits = new HashSet<Credit>();
            UserRoles = new HashSet<UserRole>();
        }

        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Address { get; set; } = null!;
        public DateTime CreatedDate { get; set; }

        public virtual ICollection<Balance> Balances { get; set; }
        public virtual ICollection<Credit> Credits { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
