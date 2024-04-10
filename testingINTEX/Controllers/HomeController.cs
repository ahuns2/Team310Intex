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

        public IActionResult Products(int page = 1, int pageSize = 10) // Add pageSize parameter
        {
            // Validate the pageSize parameter to ensure it's within a reasonable range
            pageSize = Math.Clamp(pageSize, 5, 20);

            // Calculate the number of items to skip based on the current page
            int skipAmount = (page - 1) * pageSize;

            // Retrieve a subset of products for the current page using LINQ
            var products = _context.Products
                .OrderBy(p => p.ProductId) // You can order by any property you like
                .Skip(skipAmount)
                .Take(pageSize)
                .ToList();

            // Calculate the total number of products (needed for pagination UI)
            int totalProducts = _context.Products.Count();

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
    }
}