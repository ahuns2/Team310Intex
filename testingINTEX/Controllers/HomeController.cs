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
public IActionResult Products(string[] categories, string[] colors, int page = 1, int pageSize = 10)
        {
            // Retrieve distinct categories from your data source
            var categoriesList = _repo.Products
                .SelectMany(p => new[] { p.Category1, p.Category2, p.Category3 }.AsEnumerable())
                .Distinct()
                .ToList();

            // Retrieve distinct colors from your data source
            var colorsList = _repo.Products
                .SelectMany(p => new[] { p.PrimaryColor, p.SecondaryColor }.AsEnumerable())
                .Distinct()
                .ToList();

            // Populate ViewBag.Categories and ViewBag.Colors with the category and color data
            ViewBag.Categories = categoriesList;
            ViewBag.Colors = colorsList;

            // Validate the pageSize parameter to ensure it's within a reasonable range
            pageSize = Math.Clamp(pageSize, 5, 20);

            // Retrieve the initial query for all products
            var query = _repo.Products.AsQueryable(); // Start with all products

            categories = categories.Where(c => !string.IsNullOrEmpty(c)).ToArray();

            // Apply category filters
            if (categories.Length > 0)
            {
                query = query.Where(p =>
                    categories.Any(c =>
                        (p.Category1 != null && p.Category1.Contains(c)) ||
                        (p.Category2 != null && p.Category2.Contains(c)) ||
                        (p.Category3 != null && p.Category3.Contains(c))
                    )
                );
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


        public IActionResult ItemDetails(int id)
        {
            // Retrieve the product details from your data source based on the id
            var product = _repo.Products.FirstOrDefault(p => p.ProductId == id);

            // Retrieve transactions associated with the product that have ratings
            var transactionsWithRatings = _repo.LineItems
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
            var product = _repo.Products.FirstOrDefault(p => p.ProductId == productId);
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
                    _repo.AdminAddOrder(order);

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
                _repo.Orders.Add(order);
                _repo.SaveChanges();

                // Save changes to the database
                _repo.SaveChanges();

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
        
 

        public IActionResult FraudWarning()
        {
            return View();
        }
        
    }
}


