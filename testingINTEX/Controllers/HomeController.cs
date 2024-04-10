using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using testingINTEX.Models;
using testingINTEX.Models.ViewModels;

namespace testingINTEX.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IIntexRepository _repo;
        //private readonly IntexpostgresContext _context;

        public HomeController(ILogger<HomeController> logger, IIntexRepository temp
           // IntexpostgresContext context)
           )
        {
            _logger = logger;
            _repo = temp;
            //_context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        
        //AVA: Displays the Admin Overview product page (product table)
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
                Products = _repo.Products
                    .OrderBy(x => x.Name)
                    .Skip((pageNum - 1) * pageSize)
                    .Take(pageSize)
                    .ToList(),

                //this grabs all the pagination information
                PaginationInfo = new PaginationInfo
                {
                    CurrentPage = pageNum,
                    ItemsPerPage = 10,
                    TotalItems = _repo.Products.Count()
                }
            };

            // returns product information and pagination information
            return View(viewModel);
        }
        
        //AVA: Displays the Admin single product FORM page
        [HttpGet]
        public IActionResult AdmimSingleProduct()
        {
            return View();
        }
        //AVA: The Post method that actually adds a new product to the database
        [HttpPost]
        public IActionResult AdmimSingleProduct(Product response)
        {
            _repo.AdminAddProduct(response); //add record to the database
            return View("Index");
        }
        
        
        //AVA: Get method to return the AdminSingleProduct Page WITH the data
        //of the product the admin is wanting to edit
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var recordToEdit = _repo.Products
                .Single(x => x.ProductId == id);
            return View("AdmimSingleProduct", recordToEdit);
        }
        //AVA: The Post Method that will actually update the database
        //with the change made to the product
        [HttpPost]
        public IActionResult Edit(Product updatedInfo)
        {
            _repo.AdminUpdateProduct(updatedInfo);
            return RedirectToAction("AdminProductPage");
        }
        
        //AVA: Get method to return the AdminSingleProductPage WITH the data
        //of the product the admin is wanting to delete
        [HttpGet]
        public IActionResult AdminDeleteProduct(int id)
        {
            var recordToDelete = _repo.Products
                .Single(x => x.ProductId == id);

            return View(recordToDelete);
        }
        //AVA: The Post Method that will actually delete the product
        //from the database 
        [HttpPost]
        public IActionResult AdminDeleteProduct(Product deletingInfo)
        {
            _repo.AdminDeleteProduct(deletingInfo);

            return RedirectToAction("AdminProductPage");
        }

        //CONNOR:
        public IActionResult Products(int page = 1, int pageSize = 10) // Add pageSize parameter
        {
            // Validate the pageSize parameter to ensure it's within a reasonable range
            pageSize = Math.Clamp(pageSize, 5, 20);

            // Calculate the number of items to skip based on the current page
            int skipAmount = (page - 1) * pageSize;

            // Retrieve a subset of products for the current page using LINQ
            var products = _repo.Products
                .OrderBy(p => p.ProductId) // You can order by any property you like
                .Skip(skipAmount)
                .Take(pageSize)
                .ToList();

            // Calculate the total number of products (needed for pagination UI)
            int totalProducts = _repo.Products.Count();

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