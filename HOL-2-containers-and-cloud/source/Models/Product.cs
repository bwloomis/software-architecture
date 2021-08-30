using System;
using System.Collections.Generic;

namespace myCoffeeRewards.Models
{
    public class Product
    {
        public int Id { get; set; }     // convert to GUID; note that we do not need to call it ProductId for EFCore to detect it is a primary key
        public string Name { get; set; }    // localized?
        public string SKU { get; set; }     // how to ensure this is unique?
        public long Price { get; set; }  // implied currency (USD?) and decimal (at position 2?)

        public ICollection<OrderProduct> OrdersLink { get; set; }     

        // for a real product, we'd add a lot of other information: inventory, recipe, ingredients/quantity used, average number made/day, which stores offer this product?, an image...
    } 
}