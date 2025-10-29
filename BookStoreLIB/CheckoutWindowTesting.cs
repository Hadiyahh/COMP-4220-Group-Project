using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BookStoreLIB;

namespace BookStoreLIB
{
    public static class OrderCalculator
    {
        private const decimal DefaultTaxRate = 0.13m;
        private static readonly string AddressPattern = @"^\s*\d{2,}\s+[A-Za-z]+\s+[A-Za-z]+(?:\s+.*)?$";
        public static decimal ComputeSubtotal(IEnumerable<Book> items)
        {
            if (items == null) return 0m;
            return items.Sum(b => b.Price * b.Quantity);
        }
        public static decimal ComputeTaxes(decimal subtotal, decimal taxRate = DefaultTaxRate)
        {
            return Math.Round(subtotal * taxRate, 2);
        }
        public static decimal ComputeDelivery(bool express)
        {
            return express ? 5.00m : 0.00m;
        }
        public static decimal ComputeTotal(decimal subtotal, decimal taxes, decimal delivery)
        {
            return subtotal + taxes + delivery;
        }
        public static bool IsAddressValid(string address)
        {
            return Regex.IsMatch(address ?? string.Empty, AddressPattern);
        }
        public static List<string> GetMissingFields(Dictionary<string, string> requiredFields)
        {
            return requiredFields.Where(kv => string.IsNullOrWhiteSpace(kv.Value)).Select(kv => kv.Key).ToList();
        }
    }

    [TestClass]
    public class CheckoutWindowTesting
    {
        /*private UserData userData;
        public TestContext TestContext { get; set; }

        private const string DefaultServer = "tfs.cs.uwindsor.ca";
        private const string DefaultDb = "Agile1422DB25";
        private const string LocalConnTemplate =
            "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\BookStoreDB.mdf;Integrated Security=True";

        [TestInitialize]
        public void Setup()
        {
            TryLoadDotEnv();

            var dataDir = TryFindDatabaseFolder();
            if (dataDir != null)
            {
                AppDomain.CurrentDomain.SetData("DataDirectory", dataDir);
                TestContext?.WriteLine("|DataDirectory| => " + dataDir);
            }

            var remote = BuildRemoteFromEnv();
            var local = LocalConnTemplate;

            string chosen;
            Exception remoteErr;
            if (!string.IsNullOrWhiteSpace(remote) && CanOpen(remote, 3, out remoteErr))
            {
                chosen = remote;
                TestContext?.WriteLine("DB: Using REMOTE (env-based).");
            }
            else
            {
                chosen = local;
                TestContext?.WriteLine("DB: Using LOCAL MDF.");
            }

            BookStoreLIB.Properties.Settings.Default["dbConnectionString"] = chosen;
            PrintConnectionSummary(chosen);

            userData = new UserData();
        }*/
        // to get items from cart
        private List<Book> CreateCart(params (decimal price, int qty)[] items)
        {
            var cart = new List<Book>();
            int id = 1;
            foreach (var it in items)
            {
                cart.Add(new Book
                {
                    BookID = id++,
                    Title = $"Book{id}",
                    Price = it.price,
                    Quantity = it.qty,
                    Author = "Author",
                    Year = 2000
                });
            }
            return cart;
        }

        [TestMethod]
        public void EmptyForm_ShouldPreventOrder()
        {
            var required = new Dictionary<string, string>
            {
                { "Name", string.Empty },
                { "Address", string.Empty },
                { "City", string.Empty },
                { "Province", string.Empty },
                { "Postal Code", string.Empty }
            };

            var missing = OrderCalculator.GetMissingFields(required);

            Assert.IsTrue(missing.Any(), "Expected missing required fields when shipping field(s) is empty.");
            CollectionAssert.AreEquivalent(new[] { "Name", "Address", "City", "Province", "Postal Code" }, missing);
        }

        [TestMethod]
        public void PartiallyEmptyForm_ShouldPreventOrder()
        {
            // Two fields intentionally left blank: City and Province
            var required = new Dictionary<string, string>
            {
                { "Name", "Test User" },
                { "Address", "401 University Street" },
                { "City", string.Empty },        // missing
                { "Province", string.Empty },    // missing
                { "Postal Code", "A1A1A1" }
            };

            var missing = OrderCalculator.GetMissingFields(required);

            Assert.AreEqual(2, missing.Count, "Expected exactly two missing required fields.");
            CollectionAssert.AreEquivalent(new[] { "City", "Province" }, missing);
        }

        [TestMethod]
        public void InvalidAddress_ShouldPreventOrder()
        {
            var invalidAddresses = new[]
            {
                "University Street",
                "4 Street",
                "401",
                "401UniversityStreet"
            };

            foreach (var addr in invalidAddresses)
            {
                Assert.IsFalse(OrderCalculator.IsAddressValid(addr), $"Address '{addr}' should be invalid.");
            }

            Assert.IsTrue(OrderCalculator.IsAddressValid("401 University Street"), "Expected '401 University Street' to be valid.");
        }

        [TestMethod]
        public void ExpressShipping_ShouldAddDeliveryFee()
        {
            var cart = CreateCart((10.00m, 1), (5.00m, 1));
            decimal subtotal = OrderCalculator.ComputeSubtotal(cart);
            decimal taxes = OrderCalculator.ComputeTaxes(subtotal);
            decimal delivery = OrderCalculator.ComputeDelivery(express: true);
            decimal total = OrderCalculator.ComputeTotal(subtotal, taxes, delivery);

            Assert.AreEqual(15.00m, subtotal, "Subtotal calculation incorrect.");
            Assert.AreEqual(Math.Round(15.00m * 0.13m, 2), taxes, "Taxes calculation incorrect.");
            Assert.AreEqual(5.00m, delivery, "Express delivery fee should be $5.00.");
            Assert.AreEqual(subtotal + taxes + delivery, total, "Total should include subtotal, taxes and delivery.");
        }

        [TestMethod]
        public void StandardShipping_ShouldAddDeliveryFeeOfZero()
        {
            var cart = CreateCart((10.00m, 1), (5.00m, 1));
            decimal subtotal = OrderCalculator.ComputeSubtotal(cart);
            decimal taxes = OrderCalculator.ComputeTaxes(subtotal);
            decimal delivery = OrderCalculator.ComputeDelivery(express: false);
            decimal total = OrderCalculator.ComputeTotal(subtotal, taxes, delivery);

            Assert.AreEqual(15.00m, subtotal, "Subtotal calculation incorrect.");
            Assert.AreEqual(Math.Round(15.00m * 0.13m, 2), taxes, "Taxes calculation incorrect.");
            Assert.AreEqual(0.00m, delivery, "Express delivery fee should be $5.00.");
            Assert.AreEqual(subtotal + taxes + delivery, total, "Total should include subtotal, taxes and delivery.");
        }

        //remote connection builder with ENV
        /*private static string BuildRemoteFromEnv()
        {
            var user = Environment.GetEnvironmentVariable("AGILE_DB_USER");
            var pass = Environment.GetEnvironmentVariable("AGILE_DB_PASSWORD");
            var server = Environment.GetEnvironmentVariable("AGILE_DB_SERVER") ?? DefaultServer;
            var db = Environment.GetEnvironmentVariable("AGILE_DB_NAME") ?? DefaultDb;

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass)) return null;
            var sb = new SqlConnectionStringBuilder
            {
                DataSource = server,
                InitialCatalog = db,
                PersistSecurityInfo = true,
                UserID = user,
                Password = pass,
                Encrypt = true,
                TrustServerCertificate = true
            };
            return sb.ConnectionString;
        }

        // check if we can open
        private static bool CanOpen(string connStr, int timeoutSeconds, out Exception err)
        {
            try
            {
                var sb = new SqlConnectionStringBuilder(connStr) { ConnectTimeout = timeoutSeconds };
                using (var conn = new SqlConnection(sb.ConnectionString))
                {
                    conn.Open();
                    err = null;
                    return true;
                }
            }
            catch (Exception ex)
            {
                err = ex;
                return false;
            }
        }

        // trouble shooting
        private void PrintConnectionSummary(string connStr)
        {
            try
            {
                var normalized = connStr.Replace("Trust Server Certificate", "TrustServerCertificate");
                var sb = new SqlConnectionStringBuilder(normalized);
                if (sb.ContainsKey("User ID")) sb.UserID = "***";
                if (sb.ContainsKey("Password")) sb.Password = "***";
                TestContext?.WriteLine(
                    "DB Summary -> DataSource: " + sb.DataSource +
                    ", InitialCatalog: " + sb.InitialCatalog +
                    ", AttachDbFilename: " + sb.AttachDBFilename +
                    ", Encrypt: " + sb.Encrypt +
                    ", TrustServerCertificate: " + sb.TrustServerCertificate
                );
            }
            catch (Exception ex)
            {
                var safe = connStr.Replace("Password=", "Password=***").Replace("User ID=", "User ID=***");
                TestContext?.WriteLine("DB Summary -> (raw, masked): " + safe + " | parse error: " + ex.Message);
            }
        }

        //load env from wherever we store it
        private static void TryLoadDotEnv()
        {
            try
            {
                var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                for (int i = 0; i < 6 && dir != null; i++, dir = dir.Parent)
                {
                    var envPath = Path.Combine(dir.FullName, ".env");
                    if (!File.Exists(envPath)) continue;
                    foreach (var raw in File.ReadAllLines(envPath))
                    {
                        var line = raw.Trim();
                        if (line.Length == 0 || line.StartsWith("#")) continue;
                        var idx = line.IndexOf('=');
                        if (idx <= 0) continue;
                        var key = line.Substring(0, idx).Trim();
                        var val = line.Substring(idx + 1).Trim().Trim('"');
                        Environment.SetEnvironmentVariable(key, val, EnvironmentVariableTarget.Process);
                    }
                    break;
                }
            }
            catch { }
        }

        //fallback db location checking
        private static string TryFindDatabaseFolder()
        {
            var cur = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            for (int i = 0; i < 8 && cur != null; i++, cur = cur.Parent)
            {
                var candidate = Path.Combine(cur.FullName, "BookStoreGUI", "Database");
                var mdf = Path.Combine(candidate, "BookStoreDB.mdf");
                if (File.Exists(mdf)) return candidate;
            }
            return null;
        }*/
    }
}
