using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using testingINTEX.Models;
using testingINTEX.Models;
using testingINTEX.Models.ViewModels;

namespace testingINTEX.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        // private UserManager<AppUser> _userManager;
        // private IPasswordHasher<AppUser> passwordHasher;

        private readonly IIntexRepository _repo;
        //private readonly IntexpostgresContext _context;

        public HomeController(ILogger<HomeController> logger, IIntexRepository temp
           // UserManager<AppUser> userManager, IPasswordHasher<AppUser> passwordHash, IntexpostgresContext context)
           )
        {
            _logger = logger;
            _repo = temp;
            // _userManager = userManager;
            // passwordHasher = passwordHash;
            //_context = context;
        }

        public IActionResult Index()
        {
            return RedirectToAction("LoggedInLandingPage");
        }
        
        public IActionResult Privacy()
        {
            return View();
        }
        
        public IActionResult About()
        {
            return View();
        }
        
        [Authorize(Roles = "Admin")]
        public IActionResult AdminHomePage()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        
        //
        //ADMIN USER INFORMATION:
        //
        
        public IActionResult AdminUserPageTrash()
        {
            // var users = _userManager.Users.ToList();
            // return View(users);
            return View();
        }
        
        //
        //ADMIN Product INFORMATION:
        //
        
        [Authorize(Roles = "Admin")]
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
        
        [Authorize(Roles = "Admin")]
        //AVA: Displays the Admin single product FORM page
        [HttpGet]
        public IActionResult AdmimSingleProduct()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        //AVA: The Post method that actually adds a new product to the database
        [HttpPost]
        public IActionResult AdmimSingleProduct(Product response)
        {
            _repo.AdminAddProduct(response); //add record to the database
            return View("Index");
        }
        
        [Authorize(Roles = "Admin")]
        //AVA: Get method to return the AdminSingleProduct Page WITH the data
        //of the product the admin is wanting to edit
        [HttpGet]
        public IActionResult AdminEditProduct(int id)
        {
            var recordToEdit = _repo.Products
                .Single(x => x.ProductId == id);
            return View("AdmimSingleProduct", recordToEdit);
        }
        [Authorize(Roles = "Admin")]
        //AVA: The Post Method that will actually update the database
        //with the change made to the product
        [HttpPost]
        public IActionResult AdminEditProduct(Product updatedInfo)
        {
            _repo.AdminUpdateProduct(updatedInfo);
            return RedirectToAction("AdminProductPage");
        }
        [Authorize(Roles = "Admin")]
        //AVA: Get method to return the AdminSingleProductPage WITH the data
        //of the product the admin is wanting to delete
        [HttpGet]
        public IActionResult AdminDeleteProduct(int id)
        {
            var recordToDelete = _repo.Products
                .Single(x => x.ProductId == id);

            return View(recordToDelete);
        }
        [Authorize(Roles = "Admin")]
        //AVA: The Post Method that will actually delete the product
        //from the database 
        [HttpPost]
        public IActionResult AdminDeleteProduct(Product deletingInfo)
        {
            _repo.AdminDeleteProduct(deletingInfo);

            return RedirectToAction("AdminProductPage");
        }
        
        //
        //ADMIN ORDER INFORMATION:
        //

        public IActionResult AdminOrderPage(int pageNum)
        {
            //setting how many products you want per page
            int pageSize = 100;
            // Ensure pageNum is at least 1
            if (pageNum < 1)
            {
                pageNum = 1; // Set pageNum to 1 if it's less than 1
            }

            var viewOrderModel = new OrdersListViewModel
            {
                //this grabs all the products
                Orders = _repo.Orders
                    .Where(o => o.Fraud == 1)
                    .OrderBy(x => x.Date)
                    .Take(1000)
                    .Skip((pageNum - 1) * pageSize)
                    .Take(pageSize)
                    .ToList(),

                //this grabs all the pagination information
                PaginationInfo = new PaginationInfo
                {
                    CurrentPage = pageNum,
                    ItemsPerPage = 100,
                    TotalItems = 1000
                }
            };

            // returns product information and pagination information
            return View(viewOrderModel);
        }
        
        [Authorize(Roles = "Admin")]
        //AVA: Displays the Admin single Order FORM page
        [HttpGet]
        public IActionResult AdmimSingleOrder()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        //AVA: The Post method that actually adds a new Order to the database
        [HttpPost]
        public IActionResult AdmimSingleOrder(Order response)
        {
            _repo.AdminAddOrder(response); //add record to the database
            return RedirectToAction("AdminOrderPage");
        }
        
        [Authorize(Roles = "Admin")]
        //AVA: Get method to return the AdminSingleOrder Page WITH the data
        //of the product the admin is wanting to edit
        [HttpGet]
        public IActionResult AdminEditOrder(int id)
        {
            var recordToEdit = _repo.Orders
                .Single(x => x.TransactionId == id);
            return View("AdmimSingleOrder", recordToEdit);
        }
        [Authorize(Roles = "Admin")]
        //AVA: The Post Method that will actually update the database
        //with the change made to the product
        [HttpPost]
        public IActionResult AdminEditOrder(Order updatedInfo)
        {
            _repo.AdminUpdateOrder(updatedInfo);
            return RedirectToAction("AdminOrderPage");
        }
        [Authorize(Roles = "Admin")]
        //AVA: Get method to return the AdminSingleProductPage WITH the data
        //of the product the admin is wanting to delete
        [HttpGet]
        public IActionResult AdminDeleteOrder(int id)
        {
            var recordToDelete = _repo.Orders
                .Single(x => x.TransactionId == id);

            return View("AdminDeleteOrder", recordToDelete);
        }
        [Authorize(Roles = "Admin")]
        //AVA: The Post Method that will actually delete the product
        //from the database 
        [HttpPost]
        public IActionResult AdminDeleteOrder(Order deletingInfo)
        {
            _repo.AdminDeleteOrder(deletingInfo);

            return RedirectToAction("AdminOrderPage");
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
        
        public IActionResult LandingPage()
        { 
            return View(); 
        }

        public IActionResult LoggedInLandingPage()
        {
            // Retrieve the current logged-in user's ID
            var loggedInUserId = GetCurrentUserId();

            // Retrieve the recommendations for the logged-in user
            var customer = _repo.Customers.SingleOrDefault(c => c.AspUserId == loggedInUserId);
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
            var recommendedProducts = _repo.Products.Where(p => recommendedProductIds.Contains(p.ProductId)).ToList();

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
        
        public IActionResult PurchaseConfirmation()
        {
            return View();
        }

        public IActionResult FraudWarning()
        {
            return View();
        }
        
    }
}


