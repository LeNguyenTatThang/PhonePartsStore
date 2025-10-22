using Microsoft.EntityFrameworkCore;
using PhonePartsStore.Models;

namespace PhonePartsStore.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .HasOne(l => l.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(l => l.BrandId);

            modelBuilder.Entity<Product>()
                .HasOne(l => l.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(l => l.CategoryId);


            modelBuilder.Entity<OrderDetail>()
               .HasOne(od => od.Order)
               .WithMany(o => o.OrderDetails)
               .HasForeignKey(od => od.OrderId)
               .HasPrincipalKey(o => o.OrderId);
        }
    }
}
