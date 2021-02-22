using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;// this is so the error messeages are imported
using System.ComponentModel.DataAnnotations.Schema;

namespace BankAccounts.Models
{
    public class Deposit
    {
        [Key]
        public int DepositId { get; set; }

        [Required(ErrorMessage = "Needs To Be At Least 1 to submit")]
        public decimal Amount { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}