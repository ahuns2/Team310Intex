using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using testingINTEX.Models;


namespace testingINTEX.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IntexpostgresContext _context;

        public HomeController(ILogger<HomeController> logger, IntexpostgresContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            // Retrieve all customers from the database
            var customers = _context.Customers.ToList();

            return View(customers);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Products(string[] categories, string[] colors, int page = 1, int pageSize = 10)
        {
            // Retrieve distinct categories from your data source
            var categoriesList = _context.Products
                .SelectMany(p => new[] { p.Category1, p.Category2, p.Category3 }.AsEnumerable())
                .Distinct()
                .ToList();

            // Retrieve distinct colors from your data source
            var colorsList = _context.Products
                .SelectMany(p => new[] { p.PrimaryColor, p.SecondaryColor }.AsEnumerable())
                .Distinct()
                .ToList();

            // Populate ViewBag.Categories and ViewBag.Colors with the category and color data
            ViewBag.Categories = categoriesList;
            ViewBag.Colors = colorsList;

            // Validate the pageSize parameter to ensure it's within a reasonable range
            pageSize = Math.Clamp(pageSize, 5, 20);

            // Retrieve the initial query for all products
            var query = _context.Products.AsQueryable(); // Start with all products

            // Apply category filters
            if (categories != null && categories.Length > 0)
            {
                query = query.Where(p => categories.Any(c => p.Category1.Contains(c) || p.Category2.Contains(c) || p.Category3.Contains(c)));
            }

            // Apply color filters
            if (colors != null && colors.Length > 0)
            {
                query = query.Where(p => colors.Any(c => p.PrimaryColor.Contains(c) || p.SecondaryColor.Contains(c)));
            }

            // Apply pagination
            int skipAmount = (page - 1) * pageSize;
            var products = query.OrderBy(p => p.ProductId).Skip(skipAmount).Take(pageSize).ToList();

            // Calculate the total number of products (needed for pagination UI)
            int totalProducts = query.Count();

            // Calculate the total number of pages
            int totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

            // Pass the subset of products, pagination info, and pageSize to the view
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;

            return View(products);
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        public IActionResult ItemDetails(int id)
        {
            // Retrieve the product details from your data source based on the id
            var product = _context.Products.FirstOrDefault(p => p.ProductId == id);

            // Retrieve transactions associated with the product that have ratings
            var transactionsWithRatings = _context.LineItems
                .Where(t => t.ProductId == id && t.Rating != null)
                .ToList();

            // Calculate the average rating
            double? averageRating = null;
            if (transactionsWithRatings.Any())
            {
                averageRating = transactionsWithRatings.Average(t => t.Rating);
            }

            // Pass the product and its average rating to the view
            ViewBag.AverageRating = averageRating;
            return View(product);
        }
        
        public IActionResult Cart()
        {
            // Retrieve the cart data from the session
            var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") ?? new List<CartItem>();

            // Pass the cart data to the view
            return View(cart);
        }
        
        public IActionResult Confirmation()
        {
            return View();
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
        public IActionResult PaymentTest(Order order)
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
        [HttpGet]
        public IActionResult Payment()
        {
            // Create a new Order object
            var order = new Order();
            
            int? amount = (int?)TempData["Amount"];
            ViewBag.Amount = amount;

            // Automatically populate certain fields
            order.Date = DateOnly.FromDateTime(DateTime.Now); // Assign date
            order.DayOfWeek = DateTime.Now.DayOfWeek.ToString().Substring(0,3); // Set the date to today
            order.Time = DateTime.Now.Hour; // Set the time to the current hour
            order.EntryMode = "CVV"; // Set entry mode to "CVV"
            order.TypeOfTransaction = "Online"; // Set type of transaction to "Online"
            order.Amount = amount;

            // Pass the order object to the view
            return View(order);
        }

        [HttpPost]
        public IActionResult Payment(Order order)
        {
            if (ModelState.IsValid)
            {

                int? totalCostInt = order.Amount;
                // Add the order to the database context
                _context.Orders.Add(order);

                // Save changes to the database
                _context.SaveChanges();

                // Redirect to the confirmation view
                return RedirectToAction("Confirmation", "Home");
            }

            // If the model state is not valid, return to the Payment view with validation errors
            return View("Payment", order);
        }

        public IActionResult SendToPayment(int? amount)
        {
            TempData["Amount"] = amount;
            return RedirectToAction(("Payment"));
        }
        
        
    }
}