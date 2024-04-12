using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using testingINTEX.Models;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

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

            try
            {
                _session = new InferenceSession(
                    "/Users/ahunsaker2/Documents/GitHub/testingINTEX/testingINTEX/fraud_decision_tree_model.onnx");
                _logger.LogInformation("ONNX model loaded successfully.");
            }
            catch Exception ex
            {
                _logger.LogError($"Error loading the ONNX model: {ex.Message}");
            }
        }
        
        [HttpGet]
        public IActionResult Payment()
        {
            // Create a new Order object
            var order = new Order();

            // Automatically populate certain fields
            order.Date = DateOnly.FromDateTime(DateTime.Now); // Assign date
            order.DayOfWeek = DateTime.Now.DayOfWeek.ToString().Substring(0,3); // Set the date to today
            order.Time = DateTime.Now.Hour; // Set the time to the current hour
            order.EntryMode = "CVV"; // Set entry mode to "CVV"
            order.TypeOfTransaction = "Online"; // Set type of transaction to "Online"

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
            
            try
            {
                var input = new List<float> { transaction_id, customer_id, date, day_of_week, time, entry_mode, type_of_transaction, country_of_transaction, shipping_address, bank, type_of_card };
                var inputTensor = new DenseTensor<float>(input.ToArray(), new[] { 1, input.Count });

                var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor("float_input", inputTensor)
                };

                using (var results = _session.Run(inputs)) // makes the prediction with the inputs from the form
                {
                    var prediction = results.FirstOrDefault(item => item.Name == "output_label")?.AsTensor<long>().ToArray();
                    if (prediction != null && prediction.Length > 0)
                    {
                        // Use the prediction to get the animal type from the dictionary
                        var animalType = class_type_dict.GetValueOrDefault((int)prediction[0], "Unknown");
                        ViewBag.Prediction = animalType;
                    }
                    else
                    {
                        ViewBag.Prediction = "Error: Unable to make a prediction.";
                    }
                }

                _logger.LogInformation("Prediction executed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during prediction: {ex.Message}");
                ViewBag.Prediction = "Error during prediction.";
            }

            return View("Index");
        }

            // If the model state is not valid, return to the Payment view with validation errors
            return View("Payment", order);
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
    }
}