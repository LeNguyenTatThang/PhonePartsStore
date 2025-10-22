using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhonePartsStore.Models
{
    [Table("Products")]
    public class Product
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Name { get; set; }

        public string? Description { get; set; }

        [ForeignKey("Category")]
        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("Brand")]
        [Required]
        public int BrandId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int StockQuantity { get; set; } = 0;

        [StringLength(255)]
        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public virtual Category? Category { get; set; }
        public virtual Brand? Brand { get; set; }
        public virtual ICollection<OrderDetail>? OrderDetails { get; set; }

        // Các cờ phân loại
        [NotMapped]
        public bool IsNew { get; set; } = false;
        [NotMapped]
        public bool IsFeatured { get; set; } = false;
        [NotMapped]
        public bool IsBestSeller { get; set; } = false;
    }
}