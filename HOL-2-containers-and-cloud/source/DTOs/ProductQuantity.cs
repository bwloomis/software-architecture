using System;
using System.Globalization;
using System.Collections.Generic;

namespace myCoffeeRewards.DTOs
{
    public class ProductQuantity
    {
        public string Name { get; set; } 
        public int Quantity { get; set; } 
        public long ExtendedPrice { get; set; }        
    }
}