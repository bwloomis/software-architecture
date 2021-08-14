using System;
using System.Globalization;
using System.Collections.Generic;
using myCoffeeRewards.Models;

namespace myCoffeeRewards.DTOs
{
    public class Receipt    
        // actually an order summary, if it were a receipt, it might have a link to online order, 
        // customer name, and last 4 digits of CC/token for payment reference number with bank
    {
        public int orderNumber { get; set; }
        public string storeName { get; set; }
        public List<ReceiptItem> items { get; set; }
 
        public long orderTotal { get; set; }      
        public long orderTip { get; set; }      
        public long orderTotalPaid { get; set; }      
        
        public long pointsUsed { get; set; }      
        public long pointsRemaining { get; set; }      
        public string Status { get; set; }      // note type conversion
    }

    public class ReceiptItem
    {
        public string itemName { get; set; }
        public int itemQuantity { get; set; }
        public long itemCost { get; set; }
        public long itemExtendedCost { get; set; } // itemQuantity * itemCost
    }
}