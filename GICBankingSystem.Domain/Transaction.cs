using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GICBankingSystem.Domain
{
    public class Transaction
    {
        public DateTime? Date { get; set; }
        public string? Account { get; set; }
        public string? Type { get; set; }
        public decimal? Amount { get; set; }
        public string? TransactionId { get; set; }
    }
}
