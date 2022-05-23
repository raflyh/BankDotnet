using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class Payment
    {
        public int Id { get; set; }
        public int TransactionId { get; set; }
        public int CategoryId { get; set; }

        public virtual Category Category { get; set; } = null!;
        public virtual Transaction Transaction { get; set; } = null!;
    }
}
