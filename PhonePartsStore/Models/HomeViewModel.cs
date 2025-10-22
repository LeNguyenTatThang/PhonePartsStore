using System.Collections.Generic;

namespace PhonePartsStore.Models
{
    public class HomeViewModel
    {
        public List<Product> AllProducts { get; set; } = new();
        public List<Product> NewProducts { get; set; } = new();
        public List<Product> FeaturedProducts { get; set; } = new();
        public List<Product> BestSellers { get; set; } = new();
    }
}