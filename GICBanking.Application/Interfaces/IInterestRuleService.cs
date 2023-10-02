using GICBanking.Application.Model;
using GICBankingSystem.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GICBanking.Application.Interfaces
{
    public interface IInterestRuleService
    {
        Tuple<BankDTO, bool> DefineInterestRules(BankDTO bankDTO, string ruleID, decimal rate, string date);

        Transaction CalculateInterest(BankDTO bankDTO, int year, int month, string accountNumber);
    }
}
