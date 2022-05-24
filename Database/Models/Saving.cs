using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class Saving
    {
        public int Id { get; set; }
        public int BalanceId { get; set; }
        public double TotalSaving { get; set; }
        public DateTime Date { get; set; }

        public virtual Balance Balance { get; set; } = null!;
    }
}
