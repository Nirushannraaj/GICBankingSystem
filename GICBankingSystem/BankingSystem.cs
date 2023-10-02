using GICBanking.Application.Interfaces;
using GICBanking.Application.Model;
using GICBanking.Application.Services;
using GICBankingSystem.Domain;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace GICBankingSystem
{
    public  class BankingSystem
    {
        private readonly IAccountService accountService = new AccountService();
        private readonly IInterestRuleService interestRuleService = new InterestRuleService();
        private readonly IValidator validator = new Validator();
        private static BankDTO bankDTO;

        public void InputTransactions()
        {
            Console.WriteLine("Please enter transaction details in <Date> <Account> <Type> <Amount> format " +
                "(or enter blank to go back to the main menu):");

            while (true)
            {
                string Validate = string.Empty;
                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                    break;

                string[] parts = input.Split(' ');
                if (parts.Length != 4)
                {
                    Console.WriteLine("Invalid input. Please use the specified format.");
                    continue;
                }

                string date = parts[0];
                string accountNumber = parts[1];
                string type = parts[2].ToUpper();
                decimal amount;

               
                Validate = validator.inputValidate(parts[3], "decimal");
                if(Validate != string.Empty)
                {
                    Console.WriteLine(Validate);
                    continue;
                }
                else
                {
                    decimal.TryParse(parts[3], out amount);
                }

                
                String[] strDateFormat = new String[] { "yyyyMMdd" };
                Validate = validator.inputValidate(date, "date", strDateFormat);

                if (Validate != string.Empty)
                {
                    Console.WriteLine(Validate);
                    continue;
                }
                else
                {
                    DateTime dateTimeFormat = DateTime.Now;
                    if (DateTime.TryParseExact(date, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out dateTimeFormat))
                    {
                        var result = accountService.InputTransactions(bankDTO, accountNumber, type, amount, dateTimeFormat);
                        if (result.Item2)
                        {
                            bankDTO = result.Item1 as BankDTO;
                            PrintAccountStatement(accountNumber);
                        }
                    }
                }     
               
            }

        }

        public void DefineInterestRules()
        {
            Console.WriteLine("Please enter interest rules details in <Date> <RuleId> <Rate in %> format " +
                "(or enter blank to go back to the main menu):");

            while (true)
            {
                string Validate = string.Empty;
                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                    break;

                string[] parts = input.Split(' ');
                if (parts.Length != 3)
                {
                    Console.WriteLine("Invalid input. Please use the specified format.");
                    continue;
                }

                string date = parts[0];
                string ruleId = parts[1];
                decimal rate;

                Validate = validator.inputValidate(parts[2], "rate");
                if (Validate != string.Empty)
                {
                    Console.WriteLine(Validate);
                    continue;
                }
                else
                {
                    decimal.TryParse(parts[2], out rate);
                }


                String[] strDateFormat = new String[] { "yyyyMMdd" };
                Validate = validator.inputValidate(date, "date", strDateFormat);
                if (Validate != string.Empty)
                {
                    Console.WriteLine(Validate);
                    continue;
                }

                var result = interestRuleService.DefineInterestRules(bankDTO, ruleId, rate, date);

                if (result.Item2)
                {
                    bankDTO = result.Item1 as BankDTO;
                    PrintInterestRules();
                }
            }
        }

        public void PrintAccountStatement(string accountNumber)
        {
            if (!bankDTO.accounts.ContainsKey(accountNumber))
            {
                Console.WriteLine("Account not found.");
                return;
            }

            var account = bankDTO.accounts[accountNumber];
            var transactions = account.Transactions.OrderBy(t => t.Date).ToList();
            decimal? balance = 0;

            Console.WriteLine($"Account: {accountNumber}");
            Console.WriteLine("| Date     | Txn Id      | Type | Amount | Balance |");
            foreach (var transaction in transactions)
            {
                if (transaction.Type.ToUpper() == "D")
                    balance += transaction.Amount;
                else
                    balance -= transaction.Amount;

                Console.WriteLine($"| {transaction.Date.Value.ToString("yyyyMMdd")} | {transaction.TransactionId} | {transaction.Type}    | {transaction.Amount,6:F2} | {balance,7:F2} |");
            }         

        }

        public void PrintAccountStatement(string accountNumber,string dateString)
        {
            if (!bankDTO.accounts.ContainsKey(accountNumber))
            {
                Console.WriteLine("Account not found.");
                return;
            }

            string yearString = dateString.Substring(0, 4);
            string monthString = dateString.Substring(4, 2);
            
            int year = int.Parse(yearString);
            int month = int.Parse(monthString);

            DateTime startOfMonth = new DateTime(year, month, 1);
            DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var account = bankDTO.accounts[accountNumber];
            var transactions = account.Transactions.Where(t => t.Date >= startOfMonth && t.Date <= endOfMonth).OrderBy(t => t.Date).ToList();
            decimal? balance = 0;

            balance = account.Transactions.Where(t => t.Date < startOfMonth && t.Type.ToLower() == "d")?.Sum(x => x.Amount) - account.Transactions.Where(t => t.Date < startOfMonth && t.Type.ToLower() == "w")?.Sum(x => x.Amount);
            Console.WriteLine($"Account: {accountNumber}");
            Console.WriteLine("| Date     | Txn Id      | Type | Amount | Balance |");
            foreach (var transaction in transactions)
            {
                if (transaction.Type.ToUpper() == "D")
                    balance += transaction.Amount;
                else
                    balance -= transaction.Amount;

                Console.WriteLine($"| {transaction.Date.Value.ToString("yyyyMMdd")} | {transaction.TransactionId} | {transaction.Type}    | {transaction.Amount,6:F2} | {balance,7:F2} |");
            }

            var interest = interestRuleService.CalculateInterest(bankDTO, year,month, accountNumber);
            if (interest != null)
                Console.WriteLine($"| {interest.Date.Value.ToString("yyyyMMdd")} | {interest.TransactionId} | {interest.Type}    | {interest.Amount,6:F2} | {balance + interest.Amount,7:F2} |");
        }

        public void PrintInterestRules()
        {            
            var interestRules = bankDTO.interestRules.OrderBy(t => t.Date).ToList();
            Console.WriteLine("Interest rules:");
            Console.WriteLine("| Date     | RuleId | Rate (%) |");
            foreach (var rule in interestRules)
            {
                Console.WriteLine($"| {rule.Date} | {rule.RuleId} | {rule.Rate,8:F2} |");
            }
        }

        public void PrintStatement()
        {
            Console.WriteLine("Please enter account and month to generate the statement <Account> <Year><Month>");
            Console.WriteLine("(or enter blank to go back to the main menu):");
            string input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
                return;

            string[] parts = input.Split(' ');
            if (parts.Length != 2)
            {
                Console.WriteLine("Invalid input. Please use the specified format.");
                return;
            }

            string account = parts[0];
            string yearMonth = parts[1];
            PrintAccountStatement(account, yearMonth);
        }

        public void Run()
        {
            bool firstTime = true;

            while (true)
            {
                if (firstTime)
                {
                    bankDTO = new BankDTO();
                    Console.WriteLine("Welcome to AwesomeGIC Bank! What would you like to do?");
                    firstTime = false;
                }
                else
                {
                    Console.WriteLine("Is there anything else you'd like to do?");
                }

                Console.WriteLine("[T] Input transactions");
                Console.WriteLine("[I] Define interest rules");
                Console.WriteLine("[P] Print statement");
                Console.WriteLine("[Q] Quit");
                Console.Write("> ");

                string choice = Console.ReadLine()?.ToUpper();
                switch (choice)
                {
                    case "T":
                        InputTransactions();
                        break;
                    case "I":
                        DefineInterestRules();
                        break;
                    case "P":
                        PrintStatement();
                        break;
                    case "Q":
                        Console.WriteLine("Thank you for banking with AwesomeGIC Bank.");
                        Console.WriteLine("Have a nice day!");
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please select a valid option.");
                        break;
                }
            }
        }
    }
}
