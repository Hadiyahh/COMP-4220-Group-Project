using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using BookStoreLIB;

namespace BookStoreLIB
{
    [TestClass]
    public class PaymentUnitTests
    {
        [TestMethod]
        public void GetMissingFields_AllFieldsPresent_ReturnsEmptyList()
        {
            var fields = new Dictionary<string, string>
            {
                { "Name", "Alice" },
                { "CardNumber", "1234567890123456" },
                { "CVV", "123" }
            };

            var missing = PaymentRules.GetMissingFields(fields);

            Assert.AreEqual(0, missing.Count);
        }

        [TestMethod]
        public void GetMissingFields_SomeFieldsEmpty_ReturnsCorrectKeys()
        {
            var fields = new Dictionary<string, string>
            {
                { "Name", "" },
                { "CardNumber", "1234567890123456" },
                { "CVV", null }
            };

            var missing = PaymentRules.GetMissingFields(fields);

            CollectionAssert.AreEquivalent(new List<string> { "Name", "CVV" }, missing);
        }

        [TestMethod]
        public void IsValidEmail_ValidEmail_ReturnsTrue()
        {
            Assert.IsTrue(PaymentRules.IsValidEmail("test@example.com"));
        }

        [TestMethod]
        public void IsValidEmail_InvalidEmail_ReturnsFalse()
        {
            Assert.IsFalse(PaymentRules.IsValidEmail("not-an-email"));
            Assert.IsFalse(PaymentRules.IsValidEmail(null));
            Assert.IsFalse(PaymentRules.IsValidEmail(""));
        }

        [TestMethod]
        public void IsValidCardNumber_ValidLuhn16Digits_ReturnsTrue()
        {
            // 4242424242424242 is a common test card number that passes Luhn
            Assert.IsTrue(PaymentRules.IsValidCardNumber("4242424242424242"));
        }

        [TestMethod]
        public void IsValidCardNumber_InvalidNumbers_ReturnsFalse()
        {
            Assert.IsFalse(PaymentRules.IsValidCardNumber("1234567890123456")); // fails Luhn
            Assert.IsFalse(PaymentRules.IsValidCardNumber("1234")); // too short
            Assert.IsFalse(PaymentRules.IsValidCardNumber(null));
        }

        [TestMethod]
        public void IsValidCVV_Valid3Digits_ReturnsTrue()
        {
            Assert.IsTrue(PaymentRules.IsValidCVV("123"));
        }

        [TestMethod]
        public void IsValidCVV_InvalidCVV_ReturnsFalse()
        {
            Assert.IsFalse(PaymentRules.IsValidCVV("12"));
            Assert.IsFalse(PaymentRules.IsValidCVV("abcd"));
            Assert.IsFalse(PaymentRules.IsValidCVV(null));
        }

        [TestMethod]
        public void IsValidExpiry_ValidFutureExpiry_ReturnsTrue()
        {
            string expiry = "12/99"; // safely in the future
            Assert.IsTrue(PaymentRules.IsValidExpiry(expiry, DateTime.UtcNow));
        }

        [TestMethod]
        public void IsValidExpiry_PastExpiry_ReturnsFalse()
        {
            string expiry = "01/20"; // clearly in the past
            Assert.IsFalse(PaymentRules.IsValidExpiry(expiry, DateTime.UtcNow));
        }

        [TestMethod]
        public void ComputeTotals_CalculatesCorrectly()
        {
            var items = new List<Book>
            {
                new Book { Price = 10m, Quantity = 2 },
                new Book { Price = 5m, Quantity = 1 }
            };
            decimal taxRate = 0.1m;
            decimal delivery = 3m;

            var (subtotal, taxes, total) = PaymentRules.ComputeTotals(items, taxRate, delivery);

            Assert.AreEqual(25m, subtotal); // 10*2 + 5*1
            Assert.AreEqual(2.5m, taxes);   // 10% of 25
            Assert.AreEqual(30.5m, total);  // subtotal + taxes + delivery
        }
    }
}
