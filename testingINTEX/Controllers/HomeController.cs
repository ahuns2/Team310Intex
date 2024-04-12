using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using testingINTEX.Models;
using testingINTEX.Models;
using testingINTEX.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using testingINTEX.Models;

namespace testingINTEX.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IIntexRepository _repo;
        private readonly IntexpostgresContext _context;
        // private readonly UserManager<AppUser> _userManager;
        // private readonly IPasswordHasher<AppUser> passwordHasher;
        private readonly InferenceSession _session;
        private readonly IWebHostEnvironment _env;

        public HomeController(ILogger<HomeController> logger, IIntexRepository temp, IntexpostgresContext context, IWebHostEnvironment env
            /* UserManager<AppUser> userManager, IPasswordHasher<AppUser> passwordHash */)
        {
            _logger = logger;
            _repo = temp;
            _context = context;
            // _userManager = userManager;
            // passwordHasher = passwordHash;
            _env = env;

            // Use a relative path to locate the ONNX model file based on the environment
            var modelFilePath = Path.Combine(_env.WebRootPath, "lib", "pipeline", "FraudPredictor.onnx");

            // Load the ONNX model
            _session = new InferenceSession(modelFilePath);
        }

        public class FraudPrediction
        {
            private InferenceSession session;

            public FraudPrediction(string modelPath)
            {
                session = new InferenceSession(modelPath);
                if (session == null)
                {
                    throw new Exception("Failed to load the ONNX model at: " + modelPath);
                }
            }

            public bool IsFraudulent(int[] inputData)
            {
                if (inputData == null || inputData.Length == 0)
                {
                    throw new ArgumentException("Input data for ONNX model is null or empty.");
                }

                // Convert int array to long array
                long[] longInputData = inputData.Select(i => (long)i).ToArray();

                // Create a tensor of type long (Int64)
                var inputTensor = new DenseTensor<long>(longInputData, new[] { 1, longInputData.Length });

                var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor("feature_input", inputTensor)
                };

                using (var results = session.Run(inputs))
                {
                    if (!results.Any())
                    {
                        throw new Exception("ONNX model returned no results.");
                    }

                    var resultTensor = results.First().AsTensor<long>(); // Make sure to receive it as long if needed
                    if (resultTensor == null || resultTensor.Length == 0)
                    {
                        throw new Exception("Result tensor is null or empty.");
                    }

                    int fraudResult = (int)resultTensor[0];
                    return fraudResult > 0; // Adjust this based on how the model interprets 'fraudulent'
                }
            }

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
            var product = _context.Products.FirstOrDefault(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound(); // Product not found
            }

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

            // Retrieve recommended product IDs from the current product's recommendation columns
            var recommendedProductIds = new List<int?>
            {
                product.Recommendation1,
                product.Recommendation2,
                product.Recommendation3,
                product.Recommendation4,
                product.Recommendation5
            };

            // Filter out null or zero recommendation IDs
            recommendedProductIds = recommendedProductIds.Where(id => id != null && id != 0).ToList();

            // Retrieve recommended products based on the recommendation IDs
            var recommendedProducts = _repo.Products
                .Where(p => recommendedProductIds.Contains(p.ProductId))
                .ToList();

            // Pass the product, its average rating, and recommended products to the view
            ViewBag.AverageRating = averageRating;
            ViewBag.RecommendedProducts = recommendedProducts;
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
        
        private int[] PreprocessOrderData(Order order)
        {
            // Calculate days since Jan 1, 2022
            DateOnly startDate = new DateOnly(2022, 1, 1);
            DateOnly orderDate = order.Date ?? DateOnly.FromDateTime(DateTime.Now); // Default to current date if null
            int daysSinceStart = orderDate.DayNumber - startDate.DayNumber;

            // Initialize input array for the model. Ensure the array size matches your model's expected input size.
            int[] inputData = new int[35];

            // Set days since start
            inputData[0] = daysSinceStart;

            // Assuming Time and Amount are already integers or convertible to int
            inputData[1] = order.Time ?? 0; // Assuming this is an int, directly assign
            inputData[2] = Convert.ToInt32(order.Amount); // Convert to int if Amount is not already an integer

            // Mapping string values to integers based on categories
            string[] daysOfWeek = new string[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
            for (int i = 0; i < daysOfWeek.Length; i++)
            {
                inputData[3 + i] = order.DayOfWeek == daysOfWeek[i] ? 1 : 0;
            }

            // Entry mode mapping to integers
            inputData[10] = order.EntryMode == "CVC" ? 1 : 0;
            inputData[11] = order.EntryMode == "PIN" ? 1 : 0;
            inputData[12] = order.EntryMode == "Tap" ? 1 : 0;

            // Type of transaction mapping to integers
            inputData[13] = order.TypeOfTransaction == "ATM" ? 1 : 0;
            inputData[14] = order.TypeOfTransaction == "Online" ? 1 : 0;
            inputData[15] = order.TypeOfTransaction == "POS" ? 1 : 0;

            // Country of transaction mapping to integers
            inputData[16] = order.CountryOfTransaction == "China" ? 1 : 0;
            inputData[17] = order.CountryOfTransaction == "India" ? 1 : 0;
            inputData[18] = order.CountryOfTransaction == "Russia" ? 1 : 0;
            inputData[19] = order.CountryOfTransaction == "USA" ? 1 : 0;
            inputData[20] = order.CountryOfTransaction == "United Kingdom" ? 1 : 0;

            // Shipping address mapping to integers
            inputData[21] = order.ShippingAddress == "China" ? 1 : 0;
            inputData[22] = order.ShippingAddress == "India" ? 1 : 0;
            inputData[23] = order.ShippingAddress == "Russia" ? 1 : 0;
            inputData[24] = order.ShippingAddress == "USA" ? 1 : 0;
            inputData[25] = order.ShippingAddress == "United Kingdom" ? 1 : 0;

            // Bank mapping to integers
            string[] banks = new string[] { "Barclays", "HSBC", "Halifax", "Lloyds", "Metro", "Monzo", "RBS" };
            for (int i = 0; i < banks.Length; i++)
            {
                inputData[26 + i] = order.Bank == banks[i] ? 1 : 0;
            }

            // Type of card mapping to integers
            inputData[33] = order.TypeOfCard == "MasterCard" ? 1 : 0;
            inputData[34] = order.TypeOfCard == "Visa" ? 1 : 0;

            return inputData;
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
                int[] inputData = PreprocessOrderData(order);
                

                // Use a relative path to locate the ONNX model file based on the environment
                var modelFilePath = Path.Combine(_env.WebRootPath, "lib", "pipeline", "FraudPredictor.onnx");
                var predictor = new FraudPrediction(modelFilePath);
                bool isFraudulent = predictor.IsFraudulent(inputData);

                order.Fraud = isFraudulent ? 1 : 0;
                _context.Orders.Add(order);
                _context.SaveChanges();

                if (isFraudulent)
                {
                    TempData["FraudPrediction"] = "Fraudulent Transaction Detected";
                    return RedirectToAction("Confirmation", "Home"); //GRANT FRAUD BRANCH
                }
                else
                {
                    TempData["FraudPrediction"] = "Transaction is Legitimate";
                    return RedirectToAction("Confirmation", "Home");
                }
            }

            return View(order);
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


