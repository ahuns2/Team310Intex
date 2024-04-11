using System.Diagnostics;
using System.Security.Claims;
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult CompletedSignUp()
        {
            return View();
        }
        
        public IActionResult PurchaseConfirmation()
        {
            return View();
        }

        public IActionResult FraudWarning()
        {
            return View();
        }

        public IActionResult LandingPage()
        { 
            return View(); 
        }

        public IActionResult LoggedInLandingPage()
        {
            // Retrieve the current logged-in user's ID
            var loggedInUserId = GetCurrentUserId();

            // Retrieve the recommendations for the logged-in user
            var customer = _context.Customers.SingleOrDefault(c => c.AspUserId == loggedInUserId);
            if (customer == null)
            {
                // Handle the case where the customer is not found
                return RedirectToAction("LandingPage");
            }

            // Retrieve the recommended product IDs
            var recommendedProductIds = new List<int?>
            {
                customer.Recommendation1,
                customer.Recommendation2,
                customer.Recommendation3,
                customer.Recommendation4
            };

            // Fetch the recommended products from the database
            var recommendedProducts = _context.Products.Where(p => recommendedProductIds.Contains(p.ProductId)).ToList();

            return View(recommendedProducts);
        }

        private Guid GetCurrentUserId()
        {
            // Get the user ID from the ClaimsPrincipal
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return userId;
            }

            return Guid.Empty; // Or throw an exception, depending on your requirements
        }
    }
}

