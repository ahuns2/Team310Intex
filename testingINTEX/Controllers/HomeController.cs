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
        private readonly IntexpostgresContext _context;
        private readonly InferenceSession _session;

        public HomeController(ILogger<HomeController> logger, IntexpostgresContext context)
        {
            {
                _logger = logger;
                _context = context;

                // Load the ONNX model
                _session = new InferenceSession("/Users/ahunsaker2/RiderProjects/testingINTEX/testingINTEX/FraudPredictor.onnx");
            }
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
                var predictor = new FraudPrediction("/Users/ahunsaker2/RiderProjects/testingINTEX/testingINTEX/FraudPredictor.onnx");
                bool isFraudulent = predictor.IsFraudulent(inputData);

                order.Fraud = isFraudulent ? 1 : 0;
                _context.Orders.Add(order);
                _context.SaveChanges();

                if (isFraudulent)
                {
                    TempData["FraudPrediction"] = "Fraudulent Transaction Detected";
                    return RedirectToAction("ReviewOrder", "Admin");
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
    }
}