using System;
using System.ComponentModel.DataAnnotations;

namespace jwtapi.Models
{
    public class TransactionProd
    {
        [Key]
        public int Id { get; set; }
        public Product Product { get; set; }
        public Client Client { get; set; }
        public DateTime Time { get; set; }
    }
}