using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace testingINTEX.Models
{
    public class EFIntexRepository : IIntexRepository
    {
        private readonly IntexpostgresContext _context;

        public EFIntexRepository(IntexpostgresContext context)
        {
            _context = context;
        }

        public List<Customer> Customers => _context.Customers.ToList();

        public List<LineItem> LineItems => _context.LineItems.ToList();
        
        public List<Order> Orders => _context.Orders.ToList();
        
        public List<Product> Products => _context.Products.ToList();
        
        
        public void AdminDeleteProduct(Product product)
        {
            _context.Products.Remove(product);
            _context.SaveChanges();
        }

        public void AdminAddProduct(Product product)
        {
            _context.Products.Add(product);
            _context.SaveChanges();
        }

        public void AdminUpdateProduct(Product product)
        {
            _context.Update(product);
            _context.SaveChanges();
        }

        //public void SaveChanges()
        //{
        //    _context.SaveChanges();
        //}
    }
}
