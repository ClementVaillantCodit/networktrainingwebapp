using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
using networktrainingwebapp.Models;
using System.Diagnostics;

namespace networktrainingwebapp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration Configuration;
        private readonly ILogger<HomeController> _logger;
        private readonly List<MyData> myDataList = new();

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            Configuration = configuration;
        }

        public IActionResult Index()
        {
            try
            {
                // Load data from table storage and store it in myDataList
                // Replace the following code with your actual implementation
                LoadDataFromTableStorage();
            }
            catch (Exception e)
            {
                return View(new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    ErrorMessage = $"{e.Message}{Environment.NewLine}{e.InnerException}"
                });
            }

            return View(new HomeViewModel { MyDataList = myDataList });
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

        private void LoadDataFromTableStorage()
        {
            var connectionString = Configuration["AZURE_STORAGETABLE_CONNECTIONSTRING"];
            TableServiceClient tableServiceClient = new(connectionString);

            var tableClient = tableServiceClient.GetTableClient("somedata");
            var myData = tableClient.Query<MyData>();

            foreach (var data in myData)
            {
                myDataList.Add(new MyData
                {
                    PartitionKey = data.PartitionKey,
                    RowKey = data.RowKey,
                    Timestamp = data.Timestamp
                });
            }
        }

        [HttpPost]
        public IActionResult AddData(string partitionKey, string rowKey)
        {
            var connectionString = Configuration["AZURE_STORAGETABLE_CONNECTIONSTRING"];
            TableServiceClient tableServiceClient = new(connectionString);

            var tableClient = tableServiceClient.GetTableClient("somedata");
            var myData = new MyData
            {
                PartitionKey = partitionKey,
                RowKey = rowKey,
                Timestamp = DateTimeOffset.Now
            };

            tableClient.AddEntity(myData);

            return RedirectToAction("Index");
        }
    }

    public class MyData : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}