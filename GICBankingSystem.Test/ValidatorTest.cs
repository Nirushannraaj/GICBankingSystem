using GICBanking.Application.Services;
using GICBankingSystem.Domain;
using NuGet.Frameworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GICBankingSystem.Test
{
    public class ValidatorTest
    {
        [Fact]
        public void ValidDecimalInput()
        {
            string input = "123.45";
            string type = "decimal";
            Validator validator = new Validator();

            string results = validator.inputValidate(input, type);

            Assert.True(results == string.Empty); // No error message expected for a valid decimal input
        }

        [Fact]
        public void InvalidDecimalInput_NegativeAmount()
        {
            string input = "-50.00";
            string type = "decimal";
            Validator validator = new Validator();

            string results = validator.inputValidate(input, type);

            Assert.True(results == "Invalid amount. Amount must be a positive number.");

        }

        [Fact]
        public void InvalidDecimalInput_InvalidFormat()
        {
            string input = "123.456";
            string type = "decimal";
            Validator validator = new Validator();

            string results = validator.inputValidate(input, type);

            Assert.True(results == "Invalid amount. Amount must be a 2 decimal places.");
        }

        [Fact]
        public void ValidDateInput_YYYYMMDDFormat()
        {
            string input = "20231001"; // In yyyyMMdd format
            string type = "date";
            string[] formats = { "yyyyMMdd" };
            Validator validator = new Validator();

            string results = validator.inputValidate(input, type, formats);

            Assert.True(results == string.Empty); // No error message expected for a valid date input in yyyyMMdd format
        }

        [Fact]
        public void InvalidDateInput_InvalidFormat()
        {
            string input = "01/10/2023";
            string type = "date";
            string[] formats = { "yyyyMMdd" };
            Validator validator = new Validator();

            string results = validator.inputValidate(input, type, formats);

            Assert.True(results == "Invalid Date. Date must be a Valid format : yyyyMMdd, ");
        }

        [Fact]
        public void ValidRateInput_WithInRange()
        {
            string input = "99";
            string type = "rate";
            Validator validator = new Validator();

            string results = validator.inputValidate(input, type);

            Assert.True(results == string.Empty);

        }

        [Fact]
        public void InValidRateInput_WithZero()
        {
            string input = "0";
            string type = "rate";
            Validator validator = new Validator();

            string results = validator.inputValidate(input, type);

            Assert.True(results == "Invalid interest rate. Rate must be between 0 and 100.");

        }

        [Fact]
        public void ValidRateInput_WithHundred()
        {
            string input = "100";
            string type = "rate";
            Validator validator = new Validator();

            string results = validator.inputValidate(input, type);

            Assert.True(results == string.Empty);

        }

        [Fact]
        public void InvalidRateInput_PositiveValueOutOfRange()
        {
            string input = "120";
            string type = "rate";
            Validator validator = new Validator();

            string results = validator.inputValidate(input, type);

            Assert.True(results == "Invalid interest rate. Rate must be between 0 and 100.");

        }

        [Fact]
        public void InvalidRateInput_NegativeValueOutOfRange()
        {
            string input = "-20";
            string type = "rate";
            Validator validator = new Validator();

            string results = validator.inputValidate(input, type);

            Assert.True(results == "Invalid interest rate. Rate must be between 0 and 100.");

        }

        [Fact]
        public void Validate_ValidateFirstTransaction_HappyFlow()
        {
            Validator validator = new Validator();
            var results = validator.ValidateFirstTransaction("D", new Domain.Account() { AccountNumber = "ACC0001", Balance = 0, Transactions = new List<Transaction>() });
            Assert.True(results == string.Empty);
        }

        [Fact]
        public void WithRowal_ValidateFirstTransaction_HappyFlow()
        {
            Validator validator = new Validator();
            var results = validator.ValidateFirstTransaction("w", new Domain.Account() { AccountNumber = "ACC0001", Balance = 0, Transactions = new List<Transaction>() });
            Assert.True(results == "The first transaction for an account cannot be a withdrawal.");
        }

        [Fact]
        public void FirstTransactionDepositAllowed()
        {
            Account account = new Account();
            account.Transactions = new List<Transaction>(); // Simulate an empty transaction list
            string type = "d"; // Deposit
            Validator validator = new Validator();

            string results = validator.ValidateFirstTransaction(type, account);

            Assert.True(results == string.Empty);
        }

        [Fact]
        public void FirstTransactionWithdrawalNotAllowed()
        {
            Account account = new Account();
            account.Transactions = new List<Transaction>(); // Simulate an empty transaction list
            string type = "w";
            Validator validator = new Validator();


            string result = validator.ValidateFirstTransaction(type, account);


            Assert.True(result == "The first transaction for an account cannot be a withdrawal.");
        }

        [Fact]
        public void ValidWithdrawalWithSufficientBalance()
        {
            Account account = new Account { Balance = 1000 };
            decimal amount = 500;
            string type = "w";
            Validator validator = new Validator();

            string results = validator.ValidateTransaction(account, amount, type);

            Assert.True(results == string.Empty);
        }

        [Fact]
        public void ValidWithdrawalWithExactBalance()
        {
            Account account = new Account { Balance = 1000 };
            decimal amount = 1000;
            string type = "w";
            Validator validator = new Validator();

            string results = validator.ValidateTransaction(account, amount, type);

            Assert.True(results == string.Empty);
        }

        [Fact]
        public void NonFirstTransactionWithdrawalAllowed()
        {
            Account account = new Account();
            account.Transactions = new List<Transaction>
            {
                new Transaction { Type = "d", Amount = 100 } // Simulate a previous deposit
            };
            string type = "w"; // Withdrawal
            Validator validator = new Validator();

            string results = validator.ValidateFirstTransaction(type, account);

            Assert.True(results == string.Empty); // Withdrawal allowed for non-first transaction
        }

        [Fact]
        public void NonFirstTransactionDepositAllowed()
        {
            Account account = new Account();
            account.Transactions = new List<Transaction>
            {
                new Transaction { Type = "d", Amount = 100 } // Simulate a previous deposit
            };
            string type = "d"; // Deposit
            Validator validator = new Validator();

            string results = validator.ValidateFirstTransaction(type, account);

            Assert.True(results == string.Empty); // Deposit allowed for non-first transaction
        }

        [Fact]
        public void InvalidWithdrawalWithInsufficientBalance()
        {
            Account account = new Account { Balance = 500 };
            decimal amount = 1000;
            string type = "w";
            Validator validator = new Validator();

            string results = validator.ValidateTransaction(account, amount, type);

            Assert.True(results == "Insufficient Balance. Please try a lower amount for withdrawal.");

        }



        [Fact]
        public void ValidTransactionTypeNotWithdrawal()
        {
            Account account = new Account { Balance = 1000 };
            decimal amount = 500;
            string type = "d"; // Deposit, not withdrawal
            Validator validator = new Validator();

            string results = validator.ValidateTransaction(account, amount, type);

            Assert.True(results == string.Empty);
        }

        [Fact]
        public void EdgeCaseEmptyType()
        {
            Account account = new Account { Balance = 1000 };
            decimal amount = 500;
            string type = "";
            Validator validator = new Validator();

            string results = validator.ValidateTransaction(account, amount, type);

            Assert.True(results == "Invalid transaction type.");

        }

        [Fact]
        public void EdgeCaseNegativeAmount()
        {
            Account account = new Account { Balance = 1000 };
            decimal amount = -500;
            string type = "w";
            Validator validator = new Validator();

            string results = validator.ValidateTransaction(account, amount, type);

            Assert.True(results == "Invalid transaction amount.");
        }
    }
}
