using GICBanking.Application.Interfaces;
using GICBanking.Application.Model;
using GICBankingSystem.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace GICBanking.Application.Services
{
    public class AccountService : IAccountService
    {
        private IValidator validator = new Validator();

        public Tuple<BankDTO, bool> InputTransactions(BankDTO bankDTO, string account, string type, decimal amount, DateTime date)
        {
            try
            {

                string ValidateMessage = string.Empty;

                if (!bankDTO.accounts.ContainsKey(account))
                {
                    bankDTO.accounts.Add(account, new Account { AccountNumber = account });
                }

                try
                {
                    ValidateMessage = validator.ValidateFirstTransaction(type, bankDTO.accounts[account]);
                    if (ValidateMessage != string.Empty)
                    {
                        Console.WriteLine(ValidateMessage);
                        return new(bankDTO, false);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in ValidateFirstTransaction: " + ex.Message);
                    return new Tuple<BankDTO, bool>(bankDTO, false);
                }

                try
                {
                    ValidateMessage = validator.ValidateTransaction(bankDTO.accounts[account], amount, type);
                    if (ValidateMessage != string.Empty)
                    {
                        Console.WriteLine(ValidateMessage);
                        return new(bankDTO, false);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in ValidateTransaction: " + ex.Message);
                    return new(bankDTO, false);
                }

                string TransactionId = GetNewTransactionId(bankDTO.accounts[account], date.ToString("yyyyMMdd"));
                if(TransactionId.ToUpper().Contains("ERROR"))
                {
                    return new(bankDTO, false);
                }

                bankDTO.accounts[account] = PerfomeTransaction(bankDTO.accounts[account], amount, type, date, TransactionId);

                return new(bankDTO, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                return new(bankDTO, false);
            }

        }


        private Account PerfomeTransaction(Account account, decimal amount, string type, DateTime date, string tranactionId)
        {
            try
            {
                Transaction _tempewTxn = new Transaction()
                { Account = account.AccountNumber, Type = type, Date = date, TransactionId = tranactionId, Amount = amount };

                List<Transaction> _listTranascation = account.Transactions;
                _listTranascation.Add(_tempewTxn);

                if (type.ToLower() == "w")
                {
                    account.Balance = (account.Balance ?? Convert.ToDecimal(0)) - amount;
                    account.Transactions = _listTranascation;
                }
                else if (type.ToLower() == "d")
                {
                    account.Balance = (account.Balance ?? Convert.ToDecimal(0)) + amount;
                    account.Transactions = _listTranascation;
                }
                return account;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        internal string GetNewTransactionId(Account account, string date)
        {
            try
            {
                int transactionCount = account.Transactions.Where(x => x.Date.Equals(date)).Count() + 1;
                return $"{date}-{transactionCount:D2}";
            }
            catch (Exception ex)
            {
                return "Error";
            }
        }
    }
}
