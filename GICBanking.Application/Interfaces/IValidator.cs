using GICBanking.Application.Model;
using GICBankingSystem.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GICBanking.Application.Interfaces
{
    public interface IValidator
    {
       string inputValidate(string input, string type, string[] format = null);
       string ValidateFirstTransaction(string type,Account account);
       string ValidateTransaction(Account account, decimal amount, string type);
    }
}
