using GICBanking.Application.Model;
using GICBankingSystem.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GICBanking.Application.Interfaces
{
    public interface IAccountService
    {
        Tuple<BankDTO, bool>  InputTransactions(BankDTO bankDTO,string account,string type,decimal amout, DateTime date);
    }
}
