using System;
using System.Globalization;
using System.Collections.Generic;

namespace myCoffeeRewards.Models
{
    public class Location
    {
        public int Id { get; set; }
        public string storeName { get; set; }   // what convention do we have here?  "Store0001"?  What happens with a merger?  Waht happens when we have test stores in UAT which are not physical?
        public Guid paymentProvider { get; set; }   // lookup to payment provider table - do we need multiple? preferences?

        // public Address Address { get; set; }    // Could do this to let ORM know this is a 1:1 relationship between Location and Address, Location "has" one Address
        // normally we would do this, but we're going to flatten this structure for now to avoid extra linked tables
        
        public string addressLine1 { get; set; }
        public string addressLine2 { get; set; }
        public string addressLine3 { get; set; }
        public string addressCity { get; set; }
        public string addressStateProvince { get; set; }
        public string addressCountryRegion { get; set; }
        public string postalCode { get; set; }

        // public MapCoordinates mapLocation { get; set; }  
        public decimal Altitude { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public ICollection<Order> Orders { get; set; }
    }

/*
    public class Address   // is there a standard CRM format? How do we isolate changes to this format (creating a container class)
    {
        public long AddressId { get; set; } 
        public string addressLine1 { get; set; }
        public string addressLine2 { get; set; }
        public string addressLine3 { get; set; }
        public string addressCity { get; set; }
        public string addressStateProvince { get; set; }
        public string addressCountryRegion { get; set; }
        public string postalCode { get; set; }
        // public MapCoordinates mapLocation { get; set; }

        // public long LocationRef { get; set; }      // Lets ORM know this is a 1:1 relationship between Location and Address (see OnModelCreating as well)
        public Location Location { get; set; }     // Lets ORM know this is a 1:1 relationship between Location and Address
    }

    public class MapCoordinates
    {
        // this is a custom class, many databases support spatial data types such as https://docs.microsoft.com/en-us/ef/core/modeling/spatial
        // an interesting read is how to detect the browser's location by IP address - https://blog.bitscry.com/2018/06/07/user-geolocation-in-asp-net-core/ (how would we determine the closest store to a customer?)
        public long Id { get; set; }    // not used, this could be a keyless collection
        public decimal Altitude { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }
    */
}