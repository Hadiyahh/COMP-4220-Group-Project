using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStoreLIB
{
    public class Book // for a single book
    {
        public int BookID { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal Price { get; set; }
        public int Year { get; set; }
        //public DateTime CartTimestamp { get; set; }
        public decimal Subtotal => Price * Quantity;

    }
}
