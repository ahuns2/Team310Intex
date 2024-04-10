using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using testingINTEX.Models;
using testingINTEX.Models.ViewModels;

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

        [HttpGet]
        public IActionResult AdmimSingleProduct()
        {
            return View();
        }
        [HttpPost]
        public IActionResult AdmimSingleProduct(Product response)
        {
            _context.Products.Add(response); //add record to the database
            _context.SaveChanges();
            return View("Index");
        }

        public IActionResult AdminProductPage(int pageNum)
        {
            //setting how many products you want per page
            int pageSize = 10;
            // Ensure pageNum is at least 1
            if (pageNum < 1)
            {
                pageNum = 1; // Set pageNum to 1 if it's less than 1
            }

            var viewModel = new ProductsListViewModel
            {
                //this grabs all the products
                Products = _context.Products
                    .OrderBy(x => x.Name)
                    .Skip((pageNum - 1) * pageSize)
                    .Take(pageSize)
                    .ToList(),

                //this grabs all the pagination information
                PaginationInfo = new PaginationInfo
                {
                    CurrentPage = pageNum,
                    ItemsPerPage = 10,
                    TotalItems = _context.Products.Count()
                }
            };

            // returns product information and pagination information
            return View(viewModel);
        }
        
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var recordToEdit = _context.Products
                .Single(x => x.ProductId == id);
            return View("AdmimSingleProduct", recordToEdit);
        }
        
        //This one actually edits and updates the database 
        [HttpPost]
        public IActionResult Edit(Product updatedInfo)
        {
            _context.Update(updatedInfo);
            _context.SaveChanges();
            return RedirectToAction("AdminProductPage");
        }
        [HttpGet]

        //This one displays the delete page
        public IActionResult Delete(int id)
        {
            var recordToDelete = _context.Products
                .Single(x => x.ProductId == id);

            return View("AdminProductPage");
        }

        //This one does the actual deleting
        [HttpPost]
        public IActionResult Delete(Product deletingInfo)
        {
            _context.Products.Remove(deletingInfo);
            _context.SaveChanges();

            return RedirectToAction("AdminProductPage");
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