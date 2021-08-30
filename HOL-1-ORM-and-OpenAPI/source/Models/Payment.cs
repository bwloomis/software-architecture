using System;
using System.Globalization;
using System.Collections.Generic;

namespace myCoffeeRewards.Models
{
    // simplify this... normally we'd have order 1:N with payments (could use a credit card for first $5, then loyalty rewards for remainder), 
    // but we'll say there can be only one payment per order, so 1:1 mapping
    public class Payment
    {
        public int Id { get; set; }
        public long PaymentAmount { get; set; }   // should = baseOrder + tip
        public long BaseOrderAmount { get; set; }   
        public long TipAmount { get; set; }   
        public bool UsingLoyaltyPoints { get; set; }
        public int OrderId { get; set; }    // mpas back to the order

        // public Guid TransactionToken { get; set; }  // we're not going to a payment processor, but each payment would have a token to show it was processed
        // we would also have a status of the payment here, whether it was cash or digital, etc.    ... again simplifying

        // navigation property
        public ICollection<Order> Orders;
    }
}