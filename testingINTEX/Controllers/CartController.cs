using Microsoft.AspNetCore.Mvc;
using testingINTEX.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions; // Add this line

namespace testingINTEX.Controllers
{
    public class CartController : Controller
    {
        private readonly IntexpostgresContext _context;

        public CartController(IntexpostgresContext context)
        {
            _context = context;
        }

        public IActionResult AddToCart(int productId, int quantity)
        {
            // Retrieve the product based on the productId
            var product = _context.Products.FirstOrDefault(p => p.ProductId == productId);
            if (product == null)
            {
                return NotFound(); // Product not found
            }

            // Retrieve the current cart from the session
            var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") ?? new List<CartItem>();

            // Check if the product is already in the cart
            var existingItem = cart.FirstOrDefault(item => item.Product.ProductId == productId);
            if (existingItem != null)
            {
                // Update the quantity if the product is already in the cart
                existingItem.Quantity += quantity;
            }
            else
            {
                // Add the new item to the cart
                cart.Add(new CartItem { Product = product, Quantity = quantity });
            }

            // Update the cart in the session
            HttpContext.Session.SetObject("Cart", cart);

            return RedirectToAction("Products", "Home"); // Redirect to the home page or cart page
        }

        [HttpPost]
        public IActionResult Payment(Order order)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Assign date
                    order.Date = DateOnly.FromDateTime(DateTime.Now);

                    // Assign day of week
                    order.DayOfWeek = DateTime.Now.DayOfWeek.ToString();

                    // Assign time
                    order.Time = DateTime.Now.Hour;

                    // Assign entry mode
                    order.EntryMode = "CVC";

                    // Assign type of transaction
                    order.TypeOfTransaction = "Online";

                    // Save the order to the database
                    _context.Orders.Add(order);
                    _context.SaveChanges();

                    // Redirect to the confirmation view
                    return RedirectToAction("Confirmation", "Home");
                }
                catch (Exception ex)
                {
                    // Handle exception, log error, etc.
                    ModelState.AddModelError("", "An error occurred while processing the payment.");
                }
            }

            // If ModelState is not valid, return to the payment view with errors
            return View("~/Views/Home/Payment.cshtml", order);

        }
    }
}