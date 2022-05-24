using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class Transaction
    {
        public Transaction()
        {
            Bills = new HashSet<Bill>();
        }

        public int Id { get; set; }
        public int CreditId { get; set; }
        public int BalanceId { get; set; }
        public double Total { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual Balance Balance { get; set; } = null!;
        public virtual Credit Credit { get; set; } = null!;
        public virtual ICollection<Bill> Bills { get; set; }
    }
}
