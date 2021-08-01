using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace myCoffeeRewards.Models
{
    public class Order
    {
        public Guid Id { get; set; } // unique across stores
        public DateTime OrderPlacedDateTime { get; set; }    // from POS or eCommerce site
        public DateTime OrderRequestedDateTime { get; set; }    // could be today or later in week...
        public DateTime OrderFulfilledDateTime { get; set; }  // when it reached fulfilled or cancelled status
        public Location FulfillmentLocation { get; set; }    // FK to the Location/store - in EFCore/ORM just refere to other type of the reference, to create an FK
        public List<Product> ProductsPurchased { get; set; }    // "ProductsPurchased" contains 0+ Product GUIDs (FK to the Product table), EFCore creates a mapping intermediate table
        public OrderStatus Status { get; set; }
        public Customer OrderedBy { get; set; }
        public long OrderPrice { get; set; } // may add Discount, Tip, then NetOrderPrice (OrderPrice plus Tip minus Discount); also keep amount of tax paid
        public long AmountOnPoints { get; set; }    // partial or full payment using loyalty
        public long AmountTendered { get; set; }    // partial or full payment on a credit card (may need a token to refer back to payment processor, since we do not want to keep credit card info in our database)
    } 

    public enum OrderStatus 
    {
        NOT_PURCHASED,  // includes purchase declined (no points, bad CC), other info missing; what status would need to be there if we want to allow customer to pay upon fufillment? (i.e. at the store when they pick up)
        PURCHASED_NOT_FULLFILLED,   // purchase transaction processed, order at store for fulfillment
        FULFILLING,  // order is complete - paid for, fufilled
        CANCELLED,  // includes refund/voids/returns for credit
        COMPLETED
    }
}