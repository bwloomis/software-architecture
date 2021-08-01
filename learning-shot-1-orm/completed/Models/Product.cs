using System;

namespace myCoffeeRewards.Models
{
    public class Product
    {
        public long Id { get; set; }     // convert to GUID
        public string Name { get; set; }    // localized?
        public string SKU { get; set; }     // how to ensure this is unique?
        public long Price { get; set; }  // implied currency (USD?) and decimal (at position 2?)

        // for a real product, we'd add a lot of other information: inventory, recipe, ingredients/quantity used, average number made/day, which stores offer this product?, an image...
    } 
}