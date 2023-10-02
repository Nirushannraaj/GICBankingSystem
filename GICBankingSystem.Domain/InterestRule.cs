using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GICBankingSystem.Domain
{
    public class InterestRule
    {
        public string? Date { get; set; }
        public DateTime? DateTimeFormat { get; set; }
        public string? RuleId { get; set; }
        public decimal? Rate { get; set; }
    }
}
