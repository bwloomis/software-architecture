using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace myCoffeeRewards.Models
{
    public class OrderProduct  // ORM mapping/join table
    {
        public int OrderId { get; set; }    // part of composite PK; FK to Orders
        public int ProductId { get; set; }  // part of composite PK; FK to Products
        public int Quantity { get; set; }   // for a given order, how many of that product were included (assume >= 1)
        
        // could add customizations here, like "low fat milk"

        // relationships
        public Order Order { get; set; }
        public Product Product { get; set; }   
    } 
}