using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class Credit
    {
        public Credit()
        {
            Bills = new HashSet<Bill>();
            Transactions = new HashSet<Transaction>();
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public double Limit { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime DueDate { get; set; }
        public double TotalCredit { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual ICollection<Bill> Bills { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
