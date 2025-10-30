//using System;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using BookStoreLIB;
//using BookStoreGUI;

//namespace UnitTestPayment
//{
//    [TestClass]
//    public class UnitTestPaymentValidator
//    {

//        [TestMethod]
//        public void TestMissingFields()
//        {
//            // Specify the value of test inputs 
//            string inputName = "";
//            string inputNumber = "";
//            string inputExpiry = "";
//            string inputCVV = "";
//            string inputEmail = "";

//            // Specify the value of expected outputs 
//            bool expectedReturn = false;
//            string expectedMessage = "Please fill in all fields.";

//            // Obtain the actual outputs by calling the method under testing 
//            var result = PaymentWindow.Validate(inputName, inputNumber, inputExpiry, inputCVV, inputEmail);
//            bool actualReturn = result.Item1;
//            string actualMessage = result.Item2;

//            // Verify the result 
//            Assert.AreEqual(expectedReturn, actualReturn);
//            StringAssert.Contains(actualMessage, expectedMessage);
//        }

//        [TestMethod]
//        public void TestInvalidCardDetails()
//        {
//            string inputName = "Alice";
//            string inputNumber = "41111";
//            string inputExpiry = "12/29";
//            string inputCVV = "12";
//            string inputEmail = "alice@example.com";

//            bool expectedReturn = false;
//            string expectedMessage = "Please check card details.";

//            var result = Payment.Validate(inputName, inputNumber, inputExpiry, inputCVV, inputEmail);
//            bool actualReturn = result.Item1;
//            string actualMessage = result.Item2;

//            Assert.AreEqual(expectedReturn, actualReturn);
//            StringAssert.Contains(actualMessage, expectedMessage);
//        }

//        [TestMethod]
//        public void TestInvalidEmail()
//        {
//            string inputName = "Alice";
//            string inputNumber = "4111111111111111";
//            string inputExpiry = "12/29";
//            string inputCVV = "123";
//            string inputEmail = "invalidemail";

//            bool expectedReturn = false;
//            string expectedMessage = "Invalid email.";

//            var result = Payment.Validate(inputName, inputNumber, inputExpiry, inputCVV, inputEmail);
//            bool actualReturn = result.Item1;
//            string actualMessage = result.Item2;

//            Assert.AreEqual(expectedReturn, actualReturn);
//            StringAssert.Contains(actualMessage, expectedMessage);
//        }

//        [TestMethod]
//        public void TestValidPayment()
//        {
//            string inputName = "Alice";
//            string inputNumber = "4111111111111111";
//            string inputExpiry = "12/29";
//            string inputCVV = "123";
//            string inputEmail = "alice@example.com";

//            bool expectedReturn = true;
//            string expectedMessage = "";

//            var result = Payment.Validate(inputName, inputNumber, inputExpiry, inputCVV, inputEmail);
//            bool actualReturn = result.Item1;
//            string actualMessage = result.Item2;

//            Assert.AreEqual(expectedReturn, actualReturn);
//            Assert.AreEqual(expectedMessage, actualMessage);
//        }
//    }
//}
