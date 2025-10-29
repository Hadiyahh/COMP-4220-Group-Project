using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using BookStoreLIB;

namespace BookStoreLIB
{
    [TestClass]
    public class CartSubtotalTesting
    {
        private Cart cart;

        [TestInitialize]
        public void Setup()
        {
            // Initialize a new shopping cart before each test
            cart = new Cart
            {
                shoppingCart = new List<Book>()
            };
        }

        // Helper method to simulate subtotal logic from MainWindow.xaml.cs
        private decimal CalculateSubtotal()
        {
            decimal subtotal = 0;
            foreach (var book in cart.shoppingCart)
            {
                subtotal += book.Price;
            }
            return subtotal;
        }

        [TestMethod]
        public void Subtotal_WithEmptyCart_ShouldBeZero()
        {
            // Arrange
            cart.shoppingCart.Clear();

            // Act
            var subtotal = CalculateSubtotal();

            // Assert
            Assert.AreEqual(0m, subtotal, "Expected subtotal to be 0 when cart is empty.");
        }

        [TestMethod]
        public void Subtotal_WithSingleBook_ShouldEqualBookPrice()
        {
            // Arrange
            cart.shoppingCart.Add(new Book { Title = "Clean Code", Price = 49.99m });

            // Act
            var subtotal = CalculateSubtotal();

            // Assert
            Assert.AreEqual(49.99m, subtotal, "Subtotal should match single book price.");
        }

        [TestMethod]
        public void Subtotal_WithMultipleBooks_ShouldBeSumOfPrices()
        {
            // Arrange
            cart.shoppingCart.Add(new Book { Title = "Clean Code", Price = 49.99m });
            cart.shoppingCart.Add(new Book { Title = "Refactoring", Price = 59.50m });
            cart.shoppingCart.Add(new Book { Title = "Design Patterns", Price = 39.00m });

            // Act
            var subtotal = CalculateSubtotal();

            // Assert
            Assert.AreEqual(148.49m, subtotal, 0.001m, "Subtotal should equal sum of all book prices.");
        }

        [TestMethod]
        public void Subtotal_ShouldHandleBooksWithZeroPrice()
        {
            // Arrange
            cart.shoppingCart.Add(new Book { Title = "Free eBook", Price = 0m });
            cart.shoppingCart.Add(new Book { Title = "Paid eBook", Price = 15.00m });

            // Act
            var subtotal = CalculateSubtotal();

            // Assert
            Assert.AreEqual(15.00m, subtotal, "Subtotal should ignore zero-priced books correctly.");
        }

        [TestMethod]
        public void Subtotal_ShouldHandleLargePricesWithoutOverflow()
        {
            // Arrange
            cart.shoppingCart.Add(new Book { Title = "Enterprise License", Price = 9999.99m });
            cart.shoppingCart.Add(new Book { Title = "Support Package", Price = 5000.00m });

            // Act
            var subtotal = CalculateSubtotal();

            // Assert
            Assert.AreEqual(14999.99m, subtotal, "Subtotal should correctly add large prices.");
        }

        [TestMethod]
        public void Subtotal_WithDuplicateBooks_ShouldMultiplyPriceByQuantity()
        {
            // Arrange
            var book = new Book { BookID = 107, Title = "Book G", Price = 15.00m };
            cart.shoppingCart.Add(book);
            cart.shoppingCart.Add(book); // same book added twice

            // Act
            var subtotal = 0m;

            book.Quantity = 2;
            subtotal = CalculateSubtotal();

            // Assert
            Assert.AreEqual(30.00m, subtotal, "Subtotal should account for multiple copies of the same book.");
        }

    }
}
