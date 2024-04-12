namespace testingINTEX.Models
{
    public class CartItem
    {
        public Product Product { get; set; } // Reference to the product
        public int Quantity { get; set; } // Quantity of the product in the cart
    }
}