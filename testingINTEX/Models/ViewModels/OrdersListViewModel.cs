using testingINTEX.Models;

namespace testingINTEX.Models.ViewModels
{
    public class OrdersListViewModel
    {
        //use when setting up efrepo
        // public IQueryable<Product> Products{ get; set;}
        public PaginationInfo PaginationInfo { get; set; } = new PaginationInfo();
        public List<Order> Orders { get; set; }
    }
}
