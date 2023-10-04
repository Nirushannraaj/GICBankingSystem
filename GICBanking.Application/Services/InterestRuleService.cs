using GICBanking.Application.Interfaces;
using GICBanking.Application.Model;
using GICBankingSystem.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GICBanking.Application.Services
{
    public class InterestRuleService : IInterestRuleService
    {
        public Tuple<BankDTO, bool> DefineInterestRules(BankDTO bankDTO, string ruleID, decimal rate, string date)
        {
            string ValidateMessage = string.Empty;
            DateTime dateTimeFormat = DateTime.Now;
            try
            {
                if (DateTime.TryParseExact(date, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out dateTimeFormat))
                {

                    if (!bankDTO.interestRules.Exists(x => x.Date == date))
                    {
                        bankDTO.interestRules.Add(new InterestRule { RuleId = ruleID, Rate = rate, Date = date, DateTimeFormat = dateTimeFormat });
                    }
                    else if (bankDTO.interestRules.Exists(x => x.Date == date))
                    {
                        bankDTO.interestRules.Remove(bankDTO.interestRules.Where(x => x.Date == date).FirstOrDefault());
                        bankDTO.interestRules.Add(new InterestRule { RuleId = ruleID, Rate = rate, Date = date, DateTimeFormat = dateTimeFormat });
                    }

                    return new(bankDTO, true);
                }
                else
                {
                    return new(bankDTO, false);
                }
            }
            catch (Exception ex)
            {
                ValidateMessage = "An error occurred: " + ex.Message;
                return new(bankDTO, false);
            }

        }

        public Transaction CalculateInterest(BankDTO bankDTO, int year, int month, string accountNumber)
        {
            try
            {
                Transaction instrestTransaction = new Transaction();
                decimal? collectiveIntrest = decimal.Zero;
                decimal? finalAnualIntrest = decimal.Zero;

                DateTime startOfMonth = new DateTime(year, month, 1);
                DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                for (DateTime currentDate = startOfMonth; currentDate <= endOfMonth; currentDate = currentDate.AddDays(1))
                {
                    var rate = bankDTO.interestRules.Where(x => x.DateTimeFormat <= currentDate)
                        .OrderByDescending(x => x.DateTimeFormat)?.FirstOrDefault();

                    var runningBalance = bankDTO.accounts[accountNumber].Transactions
                                             .Where(x => x.Date <= currentDate && x.Type.ToLower() == "d")
                                             ?.Sum(x => x.Amount) -
                                         bankDTO.accounts[accountNumber].Transactions
                                             .Where(x => x.Date <= currentDate && x.Type.ToLower() == "w")
                                             ?.Sum(x => x.Amount);
                    collectiveIntrest += (rate != null ? ((rate.Rate != null ? rate.Rate : 0) / 100) : 0) * (runningBalance != null ? runningBalance : 0);

                }

                finalAnualIntrest = collectiveIntrest / 365;

                instrestTransaction.TransactionId = new string(' ', 11);
                instrestTransaction.Date = endOfMonth;
                instrestTransaction.Type = "I";
                instrestTransaction.Amount = Math.Round(finalAnualIntrest.Value, 2);
                return instrestTransaction;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }       

        private class AttributeComparer : IEqualityComparer<Transaction>
        {
            public bool Equals(Transaction x, Transaction y)
            {
                try
                {
                    return x.Date == y.Date;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            public int GetHashCode(Transaction obj)
            {
                return obj.Date.GetHashCode();
            }
        }

    }
}
