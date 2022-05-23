using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class Category
    {
        public Category()
        {
            Payments = new HashSet<Payment>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string VirtualAccount { get; set; } = null!;

        public virtual ICollection<Payment> Payments { get; set; }
    }
}
