using System;
using System.Globalization;
using System.Collections.Generic;

namespace myCoffeeRewards.Models
{
    public class Customer
    {
        public int Id { get; set; } // by convention, SQL Server uses a long to be the unique key in a table, how would you change this to a GUID?  Why would you want or not want a long?

        /*
            to change this to GUID, 

            public Guid Id;

            can also do:

            [Key]
            public Guid CustomerId;  // either Id or <Type>Id work as the primary key by default, the [Key] can make any other attribute the PK as well... https://docs.microsoft.com/en-us/ef/core/modeling/keys?tabs=data-annotations
         */
        public string loginEmail { get; set; }  // unique to the account, will use later for authentication
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string mobileNumber { get; set; }    // format of +CCC (NNN) NNN-NNNN
        public long pointsEarned { get; set; }  // initialize to zero or bring over from previous system?
        public string PINCode { get; set; } // how to validate 4 digits, not longer or alpha characters?
        public DateTime lastLogin { get; set; }     // how do we specify TZ?  Do we use UTC everywhere (DateTime.UtcNow)?
        public DateTime lastCommunication { get; set; } // do we store the actual last message (FK to Communication table) in the customer profile?

        public ICollection<Order> Orders { get; set; }

        // should we add transaction history as new List<Order> or do we do a query against Orders parameterized by Customer id?
        // should we add more profile info - avatar/picture, favorite location (FK to Location), preferredLanguage (culture, "US-EN")
    }
}