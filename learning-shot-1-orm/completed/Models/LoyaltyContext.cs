using Microsoft.EntityFrameworkCore;

namespace myCoffeeRewards.Models
{
    public class LoyaltyContext : DbContext
    {
        public LoyaltyContext(DbContextOptions<LoyaltyContext> options)
            : base(options)
        {
        }

        // this sections defines the "tables" from the ERD, for the database - note the plurals, Customer_s, to represent multiple rows
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Product> Products { get; set; }
    }
}