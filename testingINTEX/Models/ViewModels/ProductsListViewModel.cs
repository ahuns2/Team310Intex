using testingINTEX.Models;

namespace testingINTEX.Models.ViewModels
{
    public class ProductsListViewModel
    {
        //use when setting up efrepo
        // public IQueryable<Product> Products{ get; set;}
        public PaginationInfo PaginationInfo { get; set; } = new PaginationInfo();
        public List<Product> Products { get; set; }
    }
}
