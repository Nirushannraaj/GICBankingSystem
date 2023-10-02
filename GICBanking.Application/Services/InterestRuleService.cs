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

                Dictionary<DateTime, DateTime> _ratePeriod = new Dictionary<DateTime, DateTime>();
                Dictionary<DateTime, DateTime> _TrnascationPair = new Dictionary<DateTime, DateTime>();
                var rateList = bankDTO.interestRules.Where(x => x.DateTimeFormat <= endOfMonth).OrderBy(x => x.DateTimeFormat).ToList();

                for (int i = 0; i < rateList.Count(); i++)
                {
                    
                    var startDate = rateList[i].DateTimeFormat.Value;
                    var EndDate = endOfMonth;
                    if (i < rateList.Count() - 1)
                    {
                        EndDate = rateList[i + 1].DateTimeFormat.Value.AddDays(-1);
                    }

                    //var periodDic = new Dictionary<DateTime, DateTime>();
                    //periodDic.Add(startDate, EndDate);
                    _ratePeriod.Add(startDate, EndDate);
                }

                var TransactionList = bankDTO.accounts[accountNumber].Transactions.Where(x => x.Date >= startOfMonth && x.Date <= endOfMonth).OrderBy(x => x.Date).Distinct(new AttributeComparer()).ToList();
                for (int i = 0; i < TransactionList.Count(); i++)
                {
                    var startDate = TransactionList[i].Date.Value;
                    var EndDate = endOfMonth;
                    if (i < TransactionList.Count() - 1)
                    {
                        EndDate = TransactionList[i + 1].Date.Value.AddDays(-1);
                    }
                    if (!_TrnascationPair.ContainsKey(startDate))
                        _TrnascationPair.Add(startDate, EndDate);
                }

                if(!_TrnascationPair.Any(x => x.Key == startOfMonth))
                {
                    _TrnascationPair.Add(startOfMonth, _TrnascationPair.Count() != 0 ? _TrnascationPair.OrderBy(x => x.Key).FirstOrDefault().Key.AddDays(-1) : endOfMonth);
                }

                foreach (var item in _TrnascationPair)
                {
                    var pivotPeriod = _ratePeriod.Where(x => x.Value >= item.Key).ToList();
                    foreach (var item2 in pivotPeriod)
                    {
                        if (item.Key >= item2.Key && item.Value >= item2.Value)
                        {
                            decimal? rate = rateList.Where(x => x.DateTimeFormat == item2.Key).FirstOrDefault()?.Rate;
                            decimal? runningBalance = bankDTO.accounts[accountNumber].Transactions.Where(x => x.Date <= item2.Value && x.Type.ToLower() == "d").Sum(y => y.Amount) - bankDTO.accounts[accountNumber].Transactions.Where(x => x.Date <= item2.Value && x.Type.ToLower() == "w").Sum(y => y.Amount);
                            TimeSpan difference = item.Key - item2.Value;
                            collectiveIntrest += ((Math.Abs(difference.Days) + 1) * (rate / 100) * runningBalance);

                        }
                        else if (item.Key <= item2.Key && item.Value >= item2.Value)
                        {
                            decimal? rate = rateList.Where(x => x.DateTimeFormat == item2.Key).FirstOrDefault()?.Rate;
                            decimal? runningBalance = bankDTO.accounts[accountNumber].Transactions.Where(x => x.Date <= item2.Value && x.Type.ToLower() == "d").Sum(y => y.Amount) - bankDTO.accounts[accountNumber].Transactions.Where(x => x.Date <= item2.Value && x.Type.ToLower() == "w").Sum(y => y.Amount);
                            TimeSpan difference = item2.Key - item2.Value;
                            collectiveIntrest += ((Math.Abs(difference.Days) + 1) * (rate / 100) * runningBalance);

                        }
                        else if (item.Key <= item2.Key && item.Value <= item2.Value)
                        {
                            decimal? rate = rateList.Where(x => x.DateTimeFormat == item2.Key).FirstOrDefault()?.Rate;
                            decimal? runningBalance = bankDTO.accounts[accountNumber].Transactions.Where(x => x.Date <= item.Value && x.Type.ToLower() == "d").Sum(y => y.Amount) - bankDTO.accounts[accountNumber].Transactions.Where(x => x.Date <= item.Value && x.Type.ToLower() == "w").Sum(y => y.Amount);
                            TimeSpan difference = item2.Key - item.Value;
                            collectiveIntrest += ((Math.Abs(difference.Days) + 1) * (rate / 100) * runningBalance);
                        }else if(item.Key >= item2.Key && item.Value <= item2.Value)
                        {
                            decimal? rate = rateList.Where(x => x.DateTimeFormat == item2.Key).FirstOrDefault()?.Rate;
                            decimal? runningBalance = bankDTO.accounts[accountNumber].Transactions.Where(x => x.Date <= item.Value &&  x.Type.ToLower() == "d").Sum(y => y.Amount) - bankDTO.accounts[accountNumber].Transactions.Where(x => x.Date <= item.Value && x.Type.ToLower() == "w").Sum(y => y.Amount);
                            TimeSpan difference = item.Key - item.Value;
                            collectiveIntrest += ((Math.Abs(difference.Days) + 1) * (rate / 100) * runningBalance);
                        }
                    }

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
