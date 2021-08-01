using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myCoffeeRewards.Models;

namespace myCoffeeRewards.Controllers
{
    [Route("myCoffeeRewards")]
    [ApiController]
    public class MyCoffeeRewardsController : ControllerBase
    {
        private readonly LoyaltyContext _context;

        public MyCoffeeRewardsController(LoyaltyContext context)
        {
            _context = context;
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