using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using myCoffeeRewards.Models;
using myCoffeeRewards.DTOs;

namespace myCoffeeRewards.Controllers
{
    [Route("myCoffeeRewards")]
    [ApiController]
    public class MyCoffeeRewardsController : ControllerBase
    {
        private readonly LoyaltyContext _context;

        public MyCoffeeRewardsController(LoyaltyContext context)
        {
            Console.WriteLine("constructor for MyCoffeeRewardsController");
            _context = context;
        }

        // GET: myCoffeeRewards/InitializeTestData
        [HttpGet("InitializeTestData")]
        public async Task<ActionResult> InitializeTestData()
        {
            Console.WriteLine("InitializeTestData()");

            MethodBase m = MethodBase.GetCurrentMethod();
            Console.WriteLine("Executing {0}.{1}", m.ReflectedType.Name, m.Name);

            SeedData();
            return Ok();
        }

        private void SeedData()
        {
            _context.Database.EnsureCreated();

            // things NOT to do:
            // _context.Customers.FromSqlRaw("DELETE FROM Customers");

            // add locations 1, 2 (if they do not already exist, add, else delete the one that is there)
            Console.WriteLine("Seeding locations");
            var testLocation = _context.Locations.FirstOrDefault(l => l.Id == 1);
            if(testLocation != null) {
                Console.WriteLine($"Found and removing location 1 ({testLocation.storeName})");
                _context.Locations.Remove(_context.Locations.Find(1)); // f => f.Id == 1); 
            }
            _context.Locations.Add(new Location { Id = 1, storeName = "FirstStore", paymentProvider = new Guid(),  
                addressLine1 = "101 Main Street", addressLine2 = "", addressLine3 = "", 
                addressCity = "Dulles", addressStateProvince = "VA", addressCountryRegion = "USA", postalCode = "10101",
                Altitude = 1.234M, Latitude = 1.000M, Longitude = 3.000M });

            testLocation = _context.Locations.FirstOrDefault(l => l.Id == 2);
            if(testLocation != null) {
                Console.WriteLine($"Found and removing location 2 ({testLocation.storeName})");
                _context.Locations.Remove(_context.Locations.Find(2));  
            }
            _context.Locations.Add(new Location { Id = 2, storeName = "SecondStore", paymentProvider = new Guid(),  
                    addressLine1 = "44 Pike Place", addressLine2 = "", addressLine3 = "", 
                    addressCity = "Seattle", addressStateProvince = "WA", addressCountryRegion = "USA", postalCode = "98052",
                    Altitude = 1.234M, Latitude = 1.000M, Longitude = 3.000M });
            
            // seed customers
            Console.WriteLine("Seeding customers");
            var testCustomer = _context.Customers.FirstOrDefault(c => c.Id == 1);
            if(testCustomer != null) {
                Console.WriteLine($"Found and removing location 1 ({testCustomer.loginEmail})");
                _context.Customers.Remove(_context.Customers.Find(1));  
            }
            _context.Customers.Add(new Customer { Id = 1, loginEmail = "brian@google.com", mobileNumber = "+01 (517) 449-1111", pointsEarned = 100, PINCode = "1234", 
                lastLogin = new DateTime(2021, 8, 1, 5, 10, 20), lastCommunication= new DateTime(2021, 7, 31, 5, 11, 20) });
                
            // seed products
            Console.WriteLine("Seeding products");
            var testProduct = _context.Products.FirstOrDefault(p => p.Id == 1);
            if(testProduct != null) {
                Console.WriteLine($"Found and removing product 1 ({testProduct.Name})");
                _context.Products.Remove(_context.Products.Find(1));  
            }
            _context.Products.Add(new Product { Id = 1, Name = "Mocha", SKU = "123-1234", Price = 500 });
            testProduct = _context.Products.FirstOrDefault(p => p.Id == 2);
            if(testProduct != null) {
                Console.WriteLine($"Found and removing product 2 ({testProduct.Name})");
                _context.Products.Remove(_context.Products.Find(2));  
            }
            _context.Products.Add(new Product { Id = 2, Name = "Americano", SKU = "123-1235", Price = 250 });

            // save your work (else the foreign keys will not actually be in the database yet)
            _context.SaveChanges();

            // seed an initial order
            Console.WriteLine("Seeding initial order");
            int maxOrderNumber = _context.Orders.Max(o => (int?) o.OrderId) ?? 1;  // .Where(o => o.Status != OrderStatus.COMPLETED)
            Customer cust = _context.Customers.FirstOrDefault(c => c.loginEmail == "brian@google.com");
            Product prod = _context.Products.FirstOrDefault(p => p.Name == "Americano");
            if(cust != null) Console.WriteLine($"cust is {cust.loginEmail}");
           
            var price = prod.Price;
            var loc = _context.Locations.FirstOrDefault(l => l.storeName == "SecondStore");
            DateTime requested = DateTime.UtcNow;
            requested = requested.AddHours(2);

            var order = new Order 
            { 
                OrderId = maxOrderNumber, 
                OrderPlacedDateTime = DateTime.UtcNow, 
                OrderRequestedDateTime = requested,
                OrderFulfilledDateTime = new DateTime(DateTime.MinValue.Ticks), 
                Status = OrderStatus.PURCHASED_NOT_FULLFILLED, 
                OrderPrice = price, 
                AmountOnPoints = 0,
                AmountTendered = price
            };
            order.Customer = cust;
            order.Location = loc;
            order.ProductsLink = new List<OrderProduct>
            {
                new OrderProduct {
                    Order = order,
                    Product = prod,
                    Quantity = 100  
                }
            };
            
           _context.Orders.Add(order);
           _context.SaveChanges(); // or .SaveChangesAsync() to thread this off
 
            foreach(var op in _context.OrderProducts.ToList()) // (a => a.Quantity > 0))
            {
                Console.WriteLine($"o={op.OrderId} p={op.ProductId} q={op.Quantity}");
            }

            Console.WriteLine("done");
            // update customer orders (we have the customer from earlier)
            // cust.Orders.Add(order);
            // _context.Update(cust);

        }

        // POST: myCoffeeRewards/CreateOrderForCustomer
        [HttpPost("CreateOrderForCustomer")]
        public async Task<ActionResult<Location>> CreateOrderForCustomer(Location location)
        {
            _context.Locations.Add(location);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLocation", new { id = location.Id }, location);
        }

        // POST: myCoffeeRewards/AddProductsToOrder
        [HttpPut("AddProductsToOrder/{id}")]
        public async Task<IActionResult> AddProductsToOrder(long id, Location location)
        {
            if (id != location.Id)
            {
                return BadRequest();
            }

            _context.Entry(location).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LocationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        // POST: myCoffeeRewards/CancelOrder
        // [Route("[controller]/CancelOrder")]
        [HttpPut("CancelOrder/{id}")]
        public async Task<IActionResult> CancelOrder(long id, Location location)
        {
            if (id != location.Id)
            {
                return BadRequest();
            }

            return NoContent();
        }

        // POST: myCoffeeRewards/PayForCustomerOrderWithCC/3
        [HttpPut("PayForCustomerOrderWithCC/{id}")]
        public async Task<IActionResult> PayForCustomerOrderWithCC(long id, Location location)
        {
            if (id != location.Id)
            {
                return BadRequest();
            }

            return NoContent();
        }

        // POST: myCoffeeRewards/PayForCustomerOrderWithPoints/2
        [HttpPut("PayForCustomerOrderWithPoints/{id}")]
        public async Task<IActionResult> PayForCustomerOrderWithPoints(long id, Location location)
        {
            if (id != location.Id)
            {
                return BadRequest();
            }

            return NoContent();
        }

        // POST: myCoffeeRewards/FulfillOrder/1
        [HttpPut("FulfillOrder/{id}")]
        public async Task<IActionResult> FulfillOrder(long id, Location location)
        {
            if (id != location.Id)
            {
                return BadRequest();
            }

            return NoContent();
        }

        // POST: myCoffeeRewards/RegisterPointsForCustomer/23
        // $10 to 1 point
        [HttpPut("RegisterPointsForCustomer/{id}")]
        public async Task<IActionResult> RegisterPointsForCustomer(long id, Location location)
        {
            if (id != location.Id)
            {
                return BadRequest();
            }

            return NoContent();
        }

        // GET: myCoffeeRewards/GetOrderStatus/7
        [HttpGet("GetCustomerNameForOrder/{id}")]
        public async Task<ActionResult<string>> GetCustomerNameForOrder(int id)
        {
            // look up the order
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            // using the order's CustomerId, look up the customer email
            var cust = await _context.Customers.FirstOrDefaultAsync(c => c.Id == order.CustomerId);
            if (cust == null)
            {
                return NotFound();
            }
            return cust.loginEmail; // TODO: need to retunr the name, not the email
        }

        // GET: myCoffeeRewards/GetProductsOnOrder/7
        [HttpGet("GetProductsOnOrder/{id}")]
        public async Task<ActionResult<List<ProductQuantity>>> GetProductsOnOrder(int id)
        {
            List<ProductQuantity> lp = new List<ProductQuantity>();

            // look up the order
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            foreach(var op in _context.OrderProducts.ToList()) // (a => a.Quantity > 0))
            {
                Console.WriteLine($"o={op.OrderId} p={op.ProductId} q={op.Quantity}");
            }

            foreach (OrderProduct op in order.ProductsLink.ToList())
            {
                int foo = op.ProductId;
                Product prod = await _context.Products.FindAsync(foo);

                Console.WriteLine($"found product {prod?.Name} with qty {op.Quantity} on the order");
                lp.Add( 
                    new ProductQuantity {
                        Name = prod.Name,
                        Quantity = op.Quantity, 
                        ExtendedPrice = op.Quantity * prod.Price
                    }
                );
            }

            return lp;
                    }
        // GET: myCoffeeRewards/GetOrderStatus/7
        [HttpGet("GetOrderStatus/{id}")]
        public async Task<ActionResult<Location>> GetOrderStatus(long id)
        {
            var location = await _context.Locations.FindAsync(id);

            if (location == null)
            {
                return NotFound();
            }

            return location;
        }

        // GET: myCoffeeRewards/GetPointsForCustomer/2
        [HttpGet("GetPointsForCustomer/{id}")]
        public async Task<ActionResult<Location>> GetPointsForCustomer(long id)
        {
            var location = await _context.Locations.FindAsync(id);

            if (location == null)
            {
                return NotFound();
            }

            return location;
        }

        // GET: myCoffeeRewards/GetTransactionsForCustomer/4
        [HttpGet("GetTransactionsForCustomer/{id}")]
        public async Task<ActionResult<Location>> GetTransactionsForCustomer(long id)
        {
            var location = await _context.Locations.FindAsync(id);

            if (location == null)
            {
                return NotFound();
            }

            return location;
        }

        // delete me when done
        private bool LocationExists(long id)
        {
            return _context.Locations.Any(e => e.Id == id);
        }

    }
}