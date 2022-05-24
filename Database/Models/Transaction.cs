using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class Transaction
    {
        public int Id { get; set; }
        public int? CreditId { get; set; }
        public int? SenderBalanceId { get; set; }
        public int? RecipientBalanceId { get; set; }
        public int? BillId { get; set; }
        public double Total { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Description { get; set; }

        public virtual Bill? Bill { get; set; }
        public virtual Credit? Credit { get; set; }
        public virtual Balance? RecipientBalance { get; set; }
        public virtual Balance? SenderBalance { get; set; }
    }
}
