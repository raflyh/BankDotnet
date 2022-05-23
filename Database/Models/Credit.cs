using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class Credit
    {
        public int Id { get; set; }
        public int BalanceId { get; set; }
        public double Limit { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime DueDate { get; set; }
        public double TotalCredit { get; set; }

        public virtual Balance Balance { get; set; } = null!;
    }
}
