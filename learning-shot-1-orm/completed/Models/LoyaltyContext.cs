using System;
using Microsoft.EntityFrameworkCore;

namespace myCoffeeRewards.Models
{
    public class LoyaltyContext : DbContext
    {
        private readonly string _connectionString;  // will hold SQL connection string later

        public LoyaltyContext(DbContextOptions<LoyaltyContext> options)
            : base(options)
        {
            _connectionString = "";
        }

        public LoyaltyContext(DbContextOptions<LoyaltyContext> options, string connectionString)
            : base(options)
        {
            _connectionString = connectionString;
        }

        // this sections defines the "tables" from the ERD, for the database - note the plurals, Customer_s, to represent multiple rows
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<OrderProduct> OrderProducts { get; set; }  // do not need to declare this, as it is an internal linking table
        public DbSet<Payment> Payments { get; set; }
        
        // override common methods per https://docs.microsoft.com/en-us/ef/core/dbcontext-configuration/
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // set up mapping table orders <--> products
            modelBuilder.Entity<OrderProduct>()             
                .HasKey(x => new {x.OrderId, x.ProductId}); 
       
            // map foreign keys (EFCore can do this by convention, but it's hidden)
            modelBuilder.Entity<OrderProduct>() 
                .HasOne(pt => pt.Product)
                .WithMany(p => p.OrdersLink)
                .HasForeignKey(pt => pt.ProductId);
 
            modelBuilder.Entity<OrderProduct>() 
                .HasOne(pt => pt.Order) 
                .WithMany(t => t.ProductsLink)
                .HasForeignKey(pt => pt.OrderId);

            // seed data - do not do this here in a real system... why not?  https://docs.microsoft.com/en-us/ef/core/modeling/data-seeding (use scripted migrations to catch schema as well as content changes, for environment-driven rather than code-first)
          
        }
    }
}