using GICBanking.Application.Interfaces;
using GICBanking.Application.Model;
using GICBankingSystem.Domain;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace GICBanking.Application.Services
{
    public class Validator : IValidator
    {
        public string inputValidate(string input, string type, string[] formats = null)
        {
            string validationmessage = String.Empty;
            try
            {
                switch (type)
                {
                    case "decimal":
                        decimal amount;
                        if (!decimal.TryParse(input, out amount) || amount <= 0)
                        {
                            validationmessage = "Invalid amount. Amount must be a positive number.";

                        }
                        else
                        {
                            var regex = new Regex(@"^\d+(\.\d{1,2})?$");
                            var flg = regex.IsMatch(input);
                            if (!flg)
                            {
                                validationmessage = "Invalid amount. Amount must be a 2 decimal places.";
                            }
                        }
                        break;

                    case "date":
                        DateTime date;
                        if (!DateTime.TryParseExact(input, formats, new CultureInfo("en-US"),
                                DateTimeStyles.None, out date))
                        {
                            validationmessage = "Invalid Date. Date must be a Valid format : ";
                            foreach (string format in formats)
                            {
                                validationmessage += format + ", ";
                            }

                        }
                        break;

                    case "rate":
                        decimal interestDate;
                        if (!decimal.TryParse(input, out interestDate) || interestDate <= 0 || interestDate > 100)
                        {
                            validationmessage = "Invalid interest rate. Rate must be between 0 and 100.";
                        }
                        break;
                }

            }
            catch (Exception ex)
            {
                validationmessage = "An error occurred while validating input." + ex.Message;
            }

            return validationmessage;
        }

        public string ValidateFirstTransaction(string type, Account account)
        {
            string Validatemessage = string.Empty;
            try
            {
                if (account.Transactions.Count == 0 && type.ToLower() == "w")
                {
                    Validatemessage = "The first transaction for an account cannot be a withdrawal.";
                }
            }
            catch (Exception ex)
            {
                Validatemessage = "An error occurred while processing the first transaction." + ex.Message;
            }

            return Validatemessage;
        }

        public string ValidateTransaction(Account account, decimal amount, string type)
        {
            string Validatemessage = string.Empty;
            try
            {

                if (type.ToLower() == "w" && (account.Balance - amount < 0))
                {
                    Validatemessage = "Insufficient Balance. Please try a lower amount for withdrawal.";
                }
                else if (type == string.Empty)
                {
                    Validatemessage = "Invalid transaction type.";
                }
                else if (amount < 0)
                {
                    Validatemessage = "Invalid transaction amount.";
                }
            }
            catch (Exception ex)
            {
                Validatemessage = "An error occurred while processing the transaction." + ex.Message;
            }

            return Validatemessage;
        }
    }
}
