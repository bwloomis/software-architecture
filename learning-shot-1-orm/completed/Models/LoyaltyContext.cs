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

        // override common methods per https://docs.microsoft.com/en-us/ef/core/dbcontext-configuration/

        /*
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }
        */

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // set up mapping table orders <--> products
            modelBuilder.Entity<OrderProduct>()             //#C
                .HasKey(x => new {x.OrderId, x.ProductId}); //#C
       
            // map foreign keys (EFCore can do this by convention, but it's hidden)
            modelBuilder.Entity<OrderProduct>() 
                .HasOne(pt => pt.Product)
                .WithMany(p => p.OrdersLink)
                .HasForeignKey(pt => pt.ProductId);
 
            modelBuilder.Entity<OrderProduct>() 
                .HasOne(pt => pt.Order) 
                .WithMany(t => t.ProductsLink)
                .HasForeignKey(pt => pt.OrderId);

            // seed data - do not do this in a real system... why not?  https://docs.microsoft.com/en-us/ef/core/modeling/data-seeding (use scripted migrations to catch schema as well as content changes, for environment-driven rather than code-first)
            // create sample customers
            /*
            modelBuilder.Entity<Customer>()
              .HasData(
               new Customer { Id = 1, loginEmail = "", mobileNumber = "", pointsEarned = 100, PINCode = "", 
               lastLogin = new DateTime(2021, 8, 1, 5, 10, 20), lastCommunication= new DateTime(2021, 7, 31, 5, 11, 20) });
            */


            // create default products
            /*
            modelBuilder.Entity<Product>()
              .HasData(
               new Product { ProductId = 1, Name = "", SKU = "", Price = 100 });
               */
        }
    }
}