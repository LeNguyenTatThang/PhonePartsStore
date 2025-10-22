using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhonePartsStore.Models
{
    [Table("Brands")]
    public class Brand
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên hãng là bắt buộc")]
        [StringLength(100)]
        public string Name { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(255)]
        public string? Description { get; set; }

        public virtual ICollection<Product>? Products { get; set; } = new List<Product>();
    }
}
