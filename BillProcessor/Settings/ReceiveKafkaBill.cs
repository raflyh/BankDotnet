using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillProcessor.Settings
{
    public class ReceiveKafkaBill
    {
        public int TransactionId { get; set; }
        public string Virtualaccount { get; set; }
        public string Bills { get; set; }
        public string PaymentStatus { get; set; }

    }
}
