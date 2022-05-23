using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class Balance
    {
        public Balance()
        {
            Credits = new HashSet<Credit>();
            Transactions = new HashSet<Transaction>();
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public string AccountNumber { get; set; } = null!;
        public double TotalBalance { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual ICollection<Credit> Credits { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
