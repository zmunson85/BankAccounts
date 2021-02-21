using System.Collections.Generic;

namespace BankAccounts.Models
{
    public class Container
    {
        public User User { get; set; }
        public List<User> AllUsers { get; set; }
        public LoginUser UserLogin { get; set; }
        public Transaction OneTransaction { get; set; }
        public List<Transaction> AllTransactions { get; set; }
    }
}