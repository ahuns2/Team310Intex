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
    }
}