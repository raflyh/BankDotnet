using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class Balance
    {
        public Balance()
        {
            Bills = new HashSet<Bill>();
            Savings = new HashSet<Saving>();
            TransactionRecipientBalances = new HashSet<Transaction>();
            TransactionSenderBalances = new HashSet<Transaction>();
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public string AccountNumber { get; set; } = null!;
        public double TotalBalance { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual ICollection<Bill> Bills { get; set; }
        public virtual ICollection<Saving> Savings { get; set; }
        public virtual ICollection<Transaction> TransactionRecipientBalances { get; set; }
        public virtual ICollection<Transaction> TransactionSenderBalances { get; set; }
    }
}
