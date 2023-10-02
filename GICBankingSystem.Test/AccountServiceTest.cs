using GICBanking.Application.Interfaces;
using GICBanking.Application.Model;
using GICBanking.Application.Services;
using GICBankingSystem.Domain;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GICBankingSystem.Test
{
    public class AccountServiceTest
    {

        [Fact]
        public void InputTransactions_WithValidData_ShouldReturnTrue()
        {
            var bankDTO = new BankDTO();
            var accountService = new AccountService();
            DateTime date = DateTime.ParseExact("20230626", "yyyyMMdd", CultureInfo.InvariantCulture);

            var results = accountService.InputTransactions(bankDTO, "AC001", "d", 100, date);

            Assert.True(results.Item2);
        }

        [Fact]
        public void InputTransactions_WithInvalidTransaction_ShouldReturnFalse()
        {
            var bankDTO = new BankDTO();
            var accoutService = new AccountService();
            DateTime date = DateTime.ParseExact("20230626", "yyyyMMdd", CultureInfo.InvariantCulture);

            var results = accoutService.InputTransactions(bankDTO, "AC001", "w", 100, date);

            Assert.False(results.Item2);
        }

        [Fact]
        public void InputTransactions_WithValidDeposit_ShouldIncreaseBalance()
        {
            var bankDTO = new BankDTO();
            var accoutService = new AccountService();
            DateTime date = DateTime.ParseExact("20230626", "yyyyMMdd", CultureInfo.InvariantCulture);

            var results = accoutService.InputTransactions(bankDTO, "AC001", "d", 100, date);

            Assert.True(results.Item2);
            Assert.Equal(100, bankDTO.accounts["AC001"].Balance);
        }

        [Fact]
        public void InputTransactions_WithInvalidWithdrawal_ShouldReturnFalse()
        {
            var bankDTO = new BankDTO();
            bankDTO.accounts.Add("AC001", new Account { AccountNumber = "AC001", Balance = 50 });

            var accoutService = new AccountService();
            DateTime date = DateTime.ParseExact("20230626", "yyyyMMdd", CultureInfo.InvariantCulture);

            var results = accoutService.InputTransactions(bankDTO, "AC001", "w", 100, date);

            Assert.False(results.Item2);
            Assert.Equal(50, bankDTO.accounts["AC001"].Balance); // Balance should not change
        }

        [Fact]
        public void InputTransactions_WithNewAccountAndValidDeposit_ShouldCreateAccountAndIncreaseBalance()
        {
            var bankDTO = new BankDTO();
            var accoutService = new AccountService();
            DateTime date = DateTime.ParseExact("20230626", "yyyyMMdd", CultureInfo.InvariantCulture);

            var results = accoutService.InputTransactions(bankDTO, "AC001", "d", 100, date);

            Assert.True(results.Item2);
            Assert.True(bankDTO.accounts.ContainsKey("AC001"));
            Assert.Equal(100, bankDTO.accounts["AC001"].Balance);

        }

        [Fact]
        public void InputTransactions_WithExistingAccountAndInvalidFirstTransaction_ShouldReturnFalse()
        {
            var bankDTO = new BankDTO();
            bankDTO.accounts.Add("AC001", new Account { AccountNumber = "AC001", Balance = 100 });
            var accoutService = new AccountService();
            DateTime date = DateTime.ParseExact("20230626", "yyyyMMdd", CultureInfo.InvariantCulture);

            var results = accoutService.InputTransactions(bankDTO, "AC001", "w", 50, date);

            Assert.False(results.Item2);
            Assert.Equal(100, bankDTO.accounts["AC001"].Balance); // Balance should not change
        }

        [Fact]
        public void InputTransactions_WithExistingAccountAndValidMultipleTransactions_ShouldUpdateBalanceCorrectly()
        {
            var bankDTO = new BankDTO();
            bankDTO.accounts.Add("AC001", new Account { AccountNumber = "AC001", Balance = 200 });
            var accoutService = new AccountService();
            DateTime date = DateTime.ParseExact("20230626", "yyyyMMdd", CultureInfo.InvariantCulture);

            accoutService.InputTransactions(bankDTO, "AC001", "d", 50, date);
            accoutService.InputTransactions(bankDTO, "AC001", "w", 30, date);
            var results = accoutService.InputTransactions(bankDTO, "AC001", "d", 70, date);

            Assert.True(results.Item2);
            Assert.Equal(290, bankDTO.accounts["AC001"].Balance);
        }


        [Fact]
        public void GenerateTransactionId_ValidInput_ReturnsExpectedFormat()
        {
            var account = new Account(); // Simulate a scenario where account is null, which may cause an exception.
            var date = "20231001";

            var accoutService = new AccountService();
            var methodInfo = typeof(AccountService).GetMethod("GetNewTransactionId", BindingFlags.NonPublic | BindingFlags.Instance);
            var result = methodInfo.Invoke(accoutService, new object[] { account, date });


            var expectedErrorMessage = "20231001-01";

            Assert.Equal(expectedErrorMessage, result);
        }
       
        [Fact]
        public void GetNewTransactionId_NoTransactionsOnDate_ReturnsExpectedFormat()
        {
            var account = new Account
            {
                Transactions = new List<Transaction>
                {
                    new Transaction { Date = DateTime.ParseExact("20231002", "yyyyMMdd", CultureInfo.InvariantCulture) },
                    new Transaction { Date = DateTime.ParseExact("20231003", "yyyyMMdd", CultureInfo.InvariantCulture) }
                }
            };
            var date = "20231001";
            var accountService = new AccountService();
            var methodInfo = typeof(AccountService).GetMethod("GetNewTransactionId", BindingFlags.NonPublic | BindingFlags.Instance);
            var result = methodInfo.Invoke(accountService, new object[] { account, date });

            var expectedErrorMessage = "20231001-01";

            Assert.Equal(expectedErrorMessage, result);
        }
    }
}

