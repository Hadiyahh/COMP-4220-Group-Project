using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.SqlClient;
using System.IO;
using BookStoreLIB;

namespace BookStoreLIB
{
    [TestClass]
    public class CartSubtotalUnitTests
    {
        private Cart Cart;
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Setup()
        {
            
        }
    }
}
