#if DEBUG
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace BookStoreLIB.Tests
{
    [TestClass]
    public class InventoryTests
    {
        [TestMethod]
        public async Task GetAll_Returns_Books()
        {
            var repo = new BookRepository();
            var rows = await repo.GetAllAsync();
            Assert.IsNotNull(rows);
            Assert.IsTrue(rows.Count > 0);
        }

        [TestMethod]
        public async Task Each_Book_Has_ISBN_Title_And_NonNegative_Stock()
        {
            var repo = new BookRepository();
            var rows = await repo.GetAllAsync();

            foreach (var b in rows)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(b.ISBN));
                Assert.IsFalse(string.IsNullOrWhiteSpace(b.Title));
                Assert.IsTrue(b.InStock >= 0);
            }
        }

        [TestMethod]
        public async Task Prices_Are_NonNegative()
        {
            var repo = new BookRepository();
            var rows = await repo.GetAllAsync();
            Assert.IsTrue(rows.All(b => b.Price >= 0m));
        }
    }
}
#endif
