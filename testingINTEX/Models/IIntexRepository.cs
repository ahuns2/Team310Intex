﻿namespace testingINTEX.Models
{
    public interface IIntexRepository
    {
        List<Customer> Customers { get; }
        
        List<LineItem> LineItems { get; }
        
        List<Order> Orders { get; }
        
        List<Product> Products { get; }
        

        public void AdminAddProduct(Product product);

        public void AdminUpdateProduct(Product product);

        public void AdminDeleteProduct(Product product);

    }
}
