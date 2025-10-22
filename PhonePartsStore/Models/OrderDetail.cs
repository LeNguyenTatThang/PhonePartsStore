using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhonePartsStore.Models
{
    [Table("OrderDetail")]
    public class OrderDetail
    {
        public int Id { get; set; } 
        public string OrderId { get; set; } 
        public int ProductId { get; set; } 
        public string ProductName { get; set; } 
        public int Quantity { get; set; } 
        public decimal Price { get; set; } 
        public decimal Total { get; set; } 

        public virtual Order Order { get; set; }
    }
}