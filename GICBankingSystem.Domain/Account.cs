namespace GICBankingSystem.Domain
{
    public class Account
    {
        public string? AccountNumber { get; set; }
        public decimal? Balance { get; set; }   
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();        
    }
}