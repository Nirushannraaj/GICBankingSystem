using GICBankingSystem.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GICBanking.Application.Model
{
    public class BankDTO
    {
        public Dictionary<string, Account> accounts { get; set; } = new Dictionary<string, Account>();
        public List<InterestRule> interestRules { get; } = new List<InterestRule>();
        
    }
}
