using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace jwtapi.Models
{
    public class TransactionProd
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }
        [JsonIgnore]
        public Product Product { get; set; }
        [ForeignKey(nameof(Client))]
        public int ClientId { get; set; }
        [JsonIgnore]
        public Client Client { get; set; }
        public DateTime Time { get; set; }
    }
}