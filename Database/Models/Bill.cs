using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class Bill
    {
        public int Id { get; set; }
        public int TransactionId { get; set; }
        public string VirtualAccount { get; set; } = null!;
        public double TotalBill { get; set; }
        public string PaymentStatus { get; set; } = null!;
        public string Type { get; set; } = null!;

        public virtual Transaction Transaction { get; set; } = null!;
    }
}
