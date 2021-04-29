using System;

namespace jwtapi.Models
{
    public class TransactionPost
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int ClientId { get; set; }
        public DateTime Time { get; set; }
    }
}