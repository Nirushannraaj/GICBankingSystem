using GICBanking.Application.Model;
using GICBanking.Application.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GICBankingSystem.Test
{
    public class InterestRuleServiceTest
    {
        [Fact]
        public void DefineInterestRules_ValidInput_ReturnsTrue()
        {            
            var interestRuleService = new InterestRuleService(); 
            var bankDTO = new BankDTO();
            var ruleID = "Rule1";
            var rate = 0.05m;
            var date = "20231001";
                        
            var result = interestRuleService.DefineInterestRules(bankDTO, ruleID, rate, date);
                       
            Assert.True(result.Item2); // Expecting true for a valid input
        }

        [Fact]
        public void DefineInterestRules_InvalidDateFormat_ReturnsFalse()
        {
            var interestRuleService = new InterestRuleService();
            var bankDTO = new BankDTO();
            var ruleID = "Rule1";
            var rate = 0.05m;
            var date = "2023-10-01"; // Invalid date format
                        
            var result = interestRuleService.DefineInterestRules(bankDTO, ruleID, rate, date);
                        
            Assert.False(result.Item2); // Expecting false for an invalid date format
        }

        [Fact]
        public void CalculateInterest_ValidInput_ReturnsTransaction()
        {
            var interestRuleService = new InterestRuleService();
            var accoutService = new AccountService();
            var bankDTO = new BankDTO(); // Populate with necessary data
            var year = 2023;
            var month = 10;
            var accountNumber = "AC001";

            accoutService.InputTransactions(bankDTO, "AC001", "d", 50, DateTime.Now);
            var result = interestRuleService.CalculateInterest(bankDTO, year, month, accountNumber);

            Assert.NotNull(result); // Expecting a valid Transaction object
            Assert.Equal("I", result.Type); // Expecting the Type to be "I" for interest transaction
        }

        [Fact]
        public void DefineInterestRules_AddNewRule_ReturnsTrue()
        {
            var interestRuleService = new InterestRuleService();
            var bankDTO = new BankDTO();
            var ruleID = "Rule1";
            var rate = 0.05m;
            var date = "20231001";

            var result = interestRuleService.DefineInterestRules(bankDTO, ruleID, rate, date);
                        
            Assert.True(result.Item2); // Expecting true for a new rule added
            Assert.Single(bankDTO.interestRules); // Expecting one rule in the list
            Assert.Equal(ruleID, bankDTO.interestRules[0].RuleId); // Expecting the correct RuleId
        }

        [Fact]
        public void DefineInterestRules_UpdateExistingRule_ReturnsTrue()
        {
            var interestRuleService = new InterestRuleService();
            var bankDTO = new BankDTO();
            var ruleID = "Rule1";
            var rate1 = 0.05m;
            var rate2 = 0.06m;
            var date = "20231001";
                       
            interestRuleService.DefineInterestRules(bankDTO, ruleID, rate1, date);
                       
            var result = interestRuleService.DefineInterestRules(bankDTO, ruleID, rate2, date);
                       
            Assert.True(result.Item2); // Expecting true for updating an existing rule
            Assert.Single(bankDTO.interestRules); // Expecting one rule in the list
            Assert.Equal(rate2, bankDTO.interestRules[0].Rate); // Expecting the updated rate
        }

        [Fact]
        public void CalculateInterest_NoTransactions_ReturnsZeroInterest()
        {
            var interestRuleService = new InterestRuleService();
            var bankDTO = new BankDTO(); // Populate with necessary data
            var year = 2023;
            var month = 10;
            var accountNumber = "AC001";
                       
            var result = interestRuleService.CalculateInterest(bankDTO, year, month, accountNumber);
                        
            Assert.NotNull(result); // Expecting a valid Transaction object
            Assert.Equal(0, result.Amount); // Expecting zero interest for no transactions
        }

        [Fact]
        public void CalculateInterest_WithTransactions_ReturnsValidInterest()
        {
            var interestRuleService = new InterestRuleService();
            var bankDTO = new BankDTO(); // Populate with necessary data
            var year = 2023;
            var month = 10;
            var accountNumber = "AC001";
          
            var accoutService = new AccountService();
            DateTime date = DateTime.ParseExact("20230626", "yyyyMMdd", CultureInfo.InvariantCulture);

            accoutService.InputTransactions(bankDTO, "AC001", "d", 50, date);
            accoutService.InputTransactions(bankDTO, "AC001", "w", 30, date);

            var result = interestRuleService.CalculateInterest(bankDTO, year, month, accountNumber);

            Assert.NotNull(result); // Expecting a valid Transaction object
            Assert.Equal("I", result.Type); // Expecting the Type to be "I" for interest transaction
                                           
        }        

        [Fact]
        public void CalculateInterest_WithMatchingRules_ReturnsValidInterest()
        {
            var interestRuleService = new InterestRuleService();
            var bankDTO = new BankDTO(); // Populate with necessary data
            var year = 2023;
            var month = 10;
            var accountNumber = "AC001";

            var accoutService = new AccountService();
            DateTime date = DateTime.ParseExact("20230626", "yyyyMMdd", CultureInfo.InvariantCulture);

            accoutService.InputTransactions(bankDTO, "AC001", "d", 500, date);
            accoutService.InputTransactions(bankDTO, "AC001", "w", 30, date);

            var result = interestRuleService.CalculateInterest(bankDTO, year, month, accountNumber);

            Assert.NotNull(result); // Expecting a valid Transaction object
            Assert.Equal("I", result.Type); // Expecting the Type to be "I" for interest transaction                                          
        }
    }
}
