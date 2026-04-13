using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GreenFieldWeb.Models;

namespace GreenFieldWeb.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<GreenFieldWeb.Models.Basket> Basket { get; set; } = default!;
        public DbSet<GreenFieldWeb.Models.BasketProducts> BasketProducts { get; set; } = default!;
        public DbSet<GreenFieldWeb.Models.Discounts> Discounts { get; set; } = default!;
        public DbSet<GreenFieldWeb.Models.OrderProducts> OrderProducts { get; set; } = default!;
        public DbSet<GreenFieldWeb.Models.Orders> Orders { get; set; } = default!;
        public DbSet<GreenFieldWeb.Models.Producers> Producers { get; set; } = default!;
        public DbSet<GreenFieldWeb.Models.Products> Products { get; set; } = default!;
    }
}
