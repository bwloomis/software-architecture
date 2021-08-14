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

        // $10 to 1 point, for any payments NOT made with loyalty points

        public MyCoffeeRewardsController(LoyaltyContext context)
        {
            _context = context;
        }

        // GET: myCoffeeRewards/InitializeTestData
        [HttpGet("InitializeTestData")]
        public ActionResult InitializeTestData()
        {
            Console.WriteLine("InitializeTestData()");

            //MethodBase m = MethodBase.GetCurrentMethod();
            //Console.WriteLine("Executing {0}.{1}", m.ReflectedType.Name, m.Name);

            SeedData();
            return Ok();
        }
        private void SeedData()
        {
            _context.Database.EnsureCreated();

            // DO NOT _context.Customers.FromSqlRaw("DELETE FROM Customers");

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

            // seed an initial payment for our order
            int maxOrderNumber = _context.Orders.Max(o => (int?)o.OrderId) ?? 0;  // .Where(o => o.Status != OrderStatus.COMPLETED)

            Console.WriteLine("Seeding payment");
            var testPayment = _context.Payments.FirstOrDefault(p => p.Id == 1);
            if(testPayment != null) {
                Console.WriteLine($"Found and removing payment 1 ({testPayment.PaymentAmount})");
                _context.Payments.Remove(_context.Payments.Find(1));  
            }
            _context.Payments.Add(new Payment
            {
                Id = 1,
                PaymentAmount = 500,
                BaseOrderAmount = 450,
                TipAmount = 50,
                OrderId = maxOrderNumber + 1,
                UsingLoyaltyPoints = false
            }
            );

            // save your work (else the foreign keys will not actually be in the database yet)
            _context.SaveChanges();

            // seed an initial order
            Console.WriteLine("Seeding initial order");
            Customer cust = _context.Customers.FirstOrDefault(c => c.loginEmail == "brian@google.com");
            Product prod = _context.Products.FirstOrDefault(p => p.Name == "Americano");
            Payment pmt = _context.Payments.FirstOrDefault(pm => pm.Id == 1);
           
            var price = prod.Price;
            var loc = _context.Locations.FirstOrDefault(l => l.storeName == "SecondStore");
            DateTime requested = DateTime.UtcNow;
            requested = requested.AddHours(2);

            var order = new Order 
            { 
                OrderId = maxOrderNumber + 1, 
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
            order.Payment = pmt;
            order.ProductsLink = new List<OrderProduct>
            {
                new OrderProduct {
                    Order = order,
                    Product = prod,
                    Quantity = 2  
                }
            };
            
            _context.Orders.Add(order);
            _context.SaveChanges(); // or .SaveChangesAsync() to thread this off

            Receipt r = createReceipt(1);
            Console.WriteLine(r.ToString());
        }
        // POST: myCoffeeRewards/CreateOrderForCustomer
        [HttpPost("CreateOrderForCustomer")]
        public async Task<ActionResult<Receipt>> CreateOrderForCustomer(Order emptyOrder, int customerId, int locationId)
        {
            // this is a function the participants in the class will implement... leave blank

            // do we need another method for anonymous orders, or ones with a nickname/phone # not entered in Loyalty/CRM?
            try
            {
                var tempOrder = _context.Orders.FirstOrDefault(o => o.OrderId == emptyOrder.OrderId);
                if(tempOrder != null)  // order already exists
                    return NotFound(emptyOrder.OrderId);

                int maxOrderNumber = _context.Orders.Max(o => (int?) o.OrderId) ?? 1;  
                Customer cust = _context.Customers.FirstOrDefault(c => c.Id == customerId);
                Location loc = _context.Locations.FirstOrDefault(c => c.Id == locationId);
                if(maxOrderNumber < 1 || cust == null || loc == null)
                    return NotFound(emptyOrder.OrderId);
                
                // emptyOrder needs to only specify OrderId = 0, OrderRequestedDateTime
                var order = new Order 
                { 
                    OrderId = maxOrderNumber + 1, 
                    OrderPlacedDateTime = DateTime.UtcNow, 
                    OrderRequestedDateTime = emptyOrder.OrderRequestedDateTime,
                    OrderFulfilledDateTime = new DateTime(DateTime.MinValue.Ticks), 
                    Status = OrderStatus.NOT_PURCHASED, 
                    OrderPrice = 0, 
                    AmountOnPoints = 0,
                    AmountTendered = 0
                };
                order.Customer = cust;
                order.Location = loc;
                
                _context.Orders.Add(order);
                await _context.SaveChangesAsync(); 

                return Ok(createReceipt(order.OrderId));
            } 
            catch(DbUpdateException e)
            {
                   return BadRequest(e);
            }
        }

        // POST: myCoffeeRewards/AddProductToOrder
        [HttpPut("AddProductToOrder")]
        public async Task<ActionResult<Receipt>> AddProductToOrder(int productId, int quantity, int orderId)
        {
            var tempOrder = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (tempOrder == null) return NotFound();
            if (tempOrder.Status == OrderStatus.NOT_PURCHASED)
            {
                // see if it's already on the order (just add to quantity)
                var tempOP = _context.OrderProducts.FirstOrDefault(op => op.OrderId == orderId && op.ProductId == productId);
                if (tempOP != null)
                {
                    tempOP.Quantity += quantity;
                    _context.OrderProducts.Update(tempOP);
                    await _context.SaveChangesAsync();

                    return Ok(createReceipt(orderId));
                }

                // else create a new OrderProduct mapping
                var tempProduct = _context.Products.FirstOrDefault(p => p.Id == productId);
                if (tempProduct == null) return NotFound();

                tempOP = new OrderProduct()
                {
                    OrderId = orderId,
                    Order = tempOrder,
                    ProductId = productId,
                    Product = tempProduct,
                    Quantity = quantity
                };
                _context.OrderProducts.Add(tempOP);
                await _context.SaveChangesAsync();

                // update order totals
                tempOrder.OrderPrice = getOrderTotal(orderId);
                _context.Orders.Update(tempOrder);
                await _context.SaveChangesAsync();

                return Ok(createReceipt(orderId));
            } else
            {
                return BadRequest("order already passed purchase stage");
            }
        }

        // POST: myCoffeeRewards/CancelOrder
        // [Route("[controller]/CancelOrder")]
        [HttpPut("CancelOrder/{id}")]
        public async Task<ActionResult> CancelOrder(int orderId)
        {
            var tempOrder = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (tempOrder == null)  // order does not exist
                return NotFound(orderId);

            switch(tempOrder.Status)
            {
                case OrderStatus.NOT_PURCHASED:
                    tempOrder.Status = OrderStatus.CANCELLED;
                    break;
                case OrderStatus.PURCHASED_NOT_FULLFILLED:
                    tempOrder.Status = OrderStatus.CANCELLED;
                    voidPayment(orderId); // return points or void/cancel purchase authorization (do not capture)
                    break;
                case OrderStatus.FULFILLING:
                    tempOrder.Status = OrderStatus.CANCELLED;
                    refundPayment(orderId); // return points or refund the payment (separate call to bank since the transaction already went through capture)
                    break;
                case OrderStatus.CANCELLED:
                    break;  // already cancelled
                case OrderStatus.COMPLETED:
                    return BadRequest();    // we'll say COMPLETED does not support cancel, this would be a refund entirely
                default:
                    break;
            }

            _context.Orders.Update(tempOrder);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // POST: myCoffeeRewards/AddPaymentMethodToOrder
        [HttpPut("AddPaymentMethodToOrder")]
        public async Task<ActionResult<Receipt>> AddPaymentMethodToOrder(int orderId, bool isPoints, long amount /* creditCardNumber */)
        {
            // just adds the payment, does not process it yet (authorize is in Submit(), caputure is in Finalize()) 
            var tempOrder = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (tempOrder == null) return NotFound();

            if (tempOrder.Status == OrderStatus.NOT_PURCHASED)
            {
                // if there is a payment, remove it (only allow one payment on each order)
                var oldPmt = _context.Payments.FirstOrDefault(p => p.Id == tempOrder.PaymentId);
                if(tempOrder.PaymentId != 0 || oldPmt != null)
                {
                    _context.Payments.Remove(_context.Payments.Find(tempOrder.PaymentId));
                    await _context.SaveChangesAsync();
                }

                int maxPaymentNumber = _context.Payments.Max(o => (int?)o.Id) ?? 0;

                Payment p = new Payment()
                {
                    Id = maxPaymentNumber + 1,
                    OrderId = tempOrder.OrderId,
                    BaseOrderAmount = getOrderTotal(orderId),
                    PaymentAmount = amount,     // we will check at authorize/submit whether this is enough payment
                    TipAmount = 0,
                    UsingLoyaltyPoints = isPoints
                };
                _context.Payments.Add(p);
                await _context.SaveChangesAsync();

                // update order with the new payment
                tempOrder.Payment = p;
                tempOrder.PaymentId = maxPaymentNumber + 1;
                _context.Orders.Update(tempOrder);
                await _context.SaveChangesAsync();

                return Ok(createReceipt(tempOrder.OrderId));
            } else
            {
                return BadRequest("order already passed purchase stage");
            }
        }

        // POST: myCoffeeRewards/SubmitOrder/1
        [HttpPut("SubmitOrder/{id}")]
        public async Task<IActionResult> SubmitOrder(int orderId)
        {
            // this is a function the participants in the class will implement... leave blank

            var tempOrder = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (tempOrder == null)  // order does not exist
                return NotFound(orderId);

            // order moves to PURCHASED_NOT_FULLFILLED
            switch (tempOrder.Status)
            {
                case OrderStatus.NOT_PURCHASED:
                    // authorizes the payment (checks balance of points available or calls bank for auth) 
                    long orderAmount = getOrderTotal(orderId);
                    tempOrder.OrderPrice = orderAmount;
                    _context.Orders.Update(tempOrder);
                    await _context.SaveChangesAsync();

                    if (authorizePayment(orderId))
                        tempOrder.Status = OrderStatus.PURCHASED_NOT_FULLFILLED;
                    else
                        return BadRequest("payment not authorized");
                    break;
                case OrderStatus.PURCHASED_NOT_FULLFILLED:
                    break;
                case OrderStatus.FULFILLING:
                    break;
                case OrderStatus.CANCELLED:
                    return BadRequest();
                case OrderStatus.COMPLETED:
                    return BadRequest();    
                default:
                    break;
            }

            _context.Orders.Update(tempOrder);
            await _context.SaveChangesAsync();

            // let the store POS know that an order is coming by following this with a call to FulfillOrder()

            return Ok();
        }
        // POST: myCoffeeRewards/FulfillOrder/1
        [HttpPut("FulfillOrder")]
        public async Task<IActionResult> FulfillOrder(int orderId)
        {
            // this is a function the participants in the class will implement... leave blank

            // typically this is where the order management system or POS is notified to fill an order (goes to make line)
            // that may happen right away or when a future date specified for order fulfillment arrives (I want a coffee at noon tomorrow)

            var tempOrder = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (tempOrder == null)  // order does not exist
                return NotFound(orderId);

            // order moves to FULLFILLING
            switch (tempOrder.Status)
            {
                case OrderStatus.NOT_PURCHASED:
                    return BadRequest();
                case OrderStatus.PURCHASED_NOT_FULLFILLED:
                    tempOrder.Status = OrderStatus.FULFILLING;  // we're making the coffee!
                    tempOrder.OrderFulfilledDateTime = DateTime.UtcNow;
                    break;
                case OrderStatus.FULFILLING:
                    break;
                case OrderStatus.CANCELLED:
                    return BadRequest();
                case OrderStatus.COMPLETED:
                    return BadRequest();
                default:
                    break;
            }

            _context.Orders.Update(tempOrder);
            await _context.SaveChangesAsync();

            return Ok();
        }
        // POST: myCoffeeRewards/CompleteOrderAndFinalizePayment/3
        [HttpPut("CompleteOrderAndFinalizePayment")]
        public async Task<IActionResult> CompleteOrderAndFinalizePayment(int orderId, long tipAmount)
        {
            
            var tempOrder = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (tempOrder == null)  // order does not exist
                return NotFound(orderId);
            
            // order moves to COMPLETED -- it is delivered, yay!
            switch (tempOrder.Status)
            {
                case OrderStatus.NOT_PURCHASED:
                    return BadRequest();
                case OrderStatus.PURCHASED_NOT_FULLFILLED:
                    return BadRequest();
                case OrderStatus.FULFILLING:
                    // completes the payment capture (deduct from customer points or call bank for credit card, gets token back)
                    if (completePayment(orderId, tipAmount))
                        tempOrder.Status = OrderStatus.COMPLETED;
                    else
                        return BadRequest("payment not completed");
                    break;
                case OrderStatus.CANCELLED:
                    return BadRequest("cancelled order");
                case OrderStatus.COMPLETED:
                    return BadRequest("already completed");
                default:
                    break;
            }

            _context.Orders.Update(tempOrder);
            await _context.SaveChangesAsync();

            return Ok();
        }
        // POST: myCoffeeRewards/RegisterPointsForCustomer
        [HttpPut("RegisterPointsForCustomer")]
        public async Task<ActionResult<long>> RegisterPointsForCustomer(int customerId, long pointsToAdd)
        {
            Customer cust = _context.Customers.FirstOrDefault(c => c.Id == customerId);
            if (cust == null) return NotFound();

            cust.pointsEarned += pointsToAdd;   // can be negative
            _context.Customers.Update(cust);
            await _context.SaveChangesAsync();

            return Ok(cust.pointsEarned);
        }
        #region utility methods
        private long ConvertCurrencyToPoints(long purchaseAmount)
        {
            if (purchaseAmount <= 0) return 0;
            return (purchaseAmount % 100)*10;   // we're not counting sub-dollar amounts    
        }
        private long ConvertPointsToCurrency(long points)
        {
            if (points <= 0) return 0;
            return points * 10;
        }
        private string GetCustomerNameForOrder(int orderId)
        {
            Order order = _context.Orders.Find(orderId);
            if (order == null) return "";

            // using the order's CustomerId, look up the customer email
            Customer cust = _context.Customers.FirstOrDefault(c => c.Id == order.CustomerId);
            if (cust == null) return "";
            return (cust.lastName + ", " + cust.firstName); // or cust.loginEmail
        }
        private OrderStatus GetOrderStatus(int orderId)
        {
            var order = _context.Orders.Find(orderId);
            if (order == null) return OrderStatus.UNDEFINED;
            return order.Status;
        }
        private long GetPointsForCustomer(int customerId)
        {
            var cust = _context.Customers.Find(customerId);
            if (cust == null) return 0;
            return cust.pointsEarned;
        }
        private long getOrderTotal(int orderId)
        {
            long runningTotal = 0;

            foreach (OrderProduct op in _context.OrderProducts.Where(op => op.OrderId == orderId))
            {
                var prod = _context.Products.Find(op.ProductId);
                if (prod != null)
                    runningTotal += op.Quantity * prod.Price;
            }

            return runningTotal;
        }
        private Receipt createReceipt(int orderId)
        {
            var order = _context.Orders.Find(orderId);
            if (order == null) return null;
            var pmt = _context.Payments.Find(order.PaymentId);
            var cust = _context.Customers.Find(order.CustomerId);
            var loc = _context.Locations.Find(order.LocationId);

            // find all qty and products
            List<ReceiptItem> receiptItems = new List<ReceiptItem>();
            foreach(OrderProduct op in _context.OrderProducts.Where(op => op.OrderId == orderId))
            {
                var prod = _context.Products.Find(op.ProductId);
                if(prod != null)
                    receiptItems.Add(new ReceiptItem { 
                        itemName = prod.Name, 
                        itemCost = prod.Price, 
                        itemQuantity = op.Quantity, 
                        itemExtendedCost = prod.Price * op.Quantity
                        }
                    );
            }

            Receipt receipt = new Receipt()
            {
                orderNumber = order.OrderId,
                Status = Enum.GetName(typeof(OrderStatus), order.Status),
                storeName = (loc != null) ? loc.storeName : "no store specified",
                items = receiptItems,
                orderTotal = order.OrderPrice,
                orderTip = (pmt != null) ? pmt.TipAmount : 0, 
                orderTotalPaid = (pmt != null) ? pmt.PaymentAmount : 0, 
                pointsRemaining = (cust != null) ? cust.pointsEarned : 0, 
                pointsUsed = order.AmountOnPoints
            };

            return receipt;
        }
        private bool authorizePayment(int orderId)
        {
            // authorizes the payment (checks balance of points available or calls bank for auth) 

            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null) return false;    // no order
            var pmt = _context.Payments.FirstOrDefault(p => p.OrderId == orderId);
            if (pmt == null) return false;    // no payment associated

            if (pmt.PaymentAmount < order.OrderPrice) return false;   // haven't submitted payment equal to the order price

            if (pmt.UsingLoyaltyPoints == true)
            {
                // does the customer have enough points in their account
                var cust = _context.Customers.FirstOrDefault(c => c.Id == order.CustomerId);
                if (cust == null) return false;     // can't use points as there is no associated customer account
                if (pmt.PaymentAmount > cust.pointsEarned) return false;    // not enough points
                return true;
            }
            else
            {
                // would call bank at this point to pre-auth a credit card
                return true;
            }
            // next else staement would be for cash, which is always accepted
        }
        private bool completePayment(int orderId, long tip)
        {
            if (tip < 0) return false;

            // completes the payment capture (deduct from customer points or call bank for credit card - empty method - always returns true/some token)
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null) return false;    // no order
            var pmt = _context.Payments.FirstOrDefault(p => p.OrderId == orderId);
            if (pmt == null) return false;    // no payment associated

            if (pmt.UsingLoyaltyPoints == true)
            {
                // finalize totals and subtract transaction from points in account
                var cust = _context.Customers.FirstOrDefault(c => c.Id == order.CustomerId);
                if (cust != null)    // not an anonymous purchase
                {
                    order.AmountTendered = order.OrderPrice + tip;
                    order.AmountOnPoints = order.OrderPrice + tip;
                    pmt.BaseOrderAmount = order.OrderPrice;
                    pmt.TipAmount = tip;
                    pmt.PaymentAmount = order.OrderPrice + tip;

                    _context.Orders.Update(order);
                    _context.Payments.Update(pmt);
                    _context.SaveChanges();

                    cust.pointsEarned -= order.OrderPrice + tip;
                    if (cust.pointsEarned < 0) cust.pointsEarned = 0;
                    _context.Customers.Update(cust);
                    _context.SaveChanges();

                    return true;
                }
                else return false;  // no points account associated with order
            }
            else
            {
                // finalize totals and capture transaction with bank
                order.AmountTendered = order.OrderPrice + tip;
                order.AmountOnPoints = 0;
                pmt.BaseOrderAmount = order.OrderPrice;
                pmt.TipAmount = tip;
                pmt.PaymentAmount = order.OrderPrice + tip;

                _context.Orders.Update(order);
                _context.Payments.Update(pmt);
                _context.SaveChanges();
                
                // since this was a purchase, add points to customer account
                var cust = _context.Customers.FirstOrDefault(c => c.Id == order.CustomerId);
                if(cust != null)    // not an anonymous purchase
                {
                    cust.pointsEarned += ConvertCurrencyToPoints(order.OrderPrice); // not on the tip?
                    _context.Customers.Update(cust);
                    _context.SaveChanges();
                    Console.WriteLine($"added {ConvertCurrencyToPoints(order.OrderPrice)} points to {GetCustomerNameForOrder(orderId)}'s account, now at {cust.pointsEarned}");
                }

                return true;
            }
        }
        private void voidPayment(int orderId)  // return points or void/cancel purchase authorization (not captured yet)
        {
            return; // we'll leave this as an exercise for the reader
        }
        private void refundPayment(int orderId) // reverse the payment on points back to customer account, send reversal to bank
        {
            return;
        }
#endregion
    }
}