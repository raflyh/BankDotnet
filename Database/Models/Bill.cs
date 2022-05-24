using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class Bill
    {
        public Bill()
        {
            Transactions = new HashSet<Transaction>();
        }

        public int Id { get; set; }
        public int? CreditId { get; set; }
        public int? BalanceId { get; set; }
        public string VirtualAccount { get; set; } = null!;
        public double TotalBill { get; set; }
        public string? PaymentStatus { get; set; }
        public string Type { get; set; } = null!;

        public virtual Balance? Balance { get; set; }
        public virtual Credit? Credit { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
