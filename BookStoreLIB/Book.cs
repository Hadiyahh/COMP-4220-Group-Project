using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStoreLIB
{
    public class Book 
    {
        public string ISBN { get; set; }
        public int CategoryID { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public decimal Price { get; set; }
        public int SupplierId { get; set; }
        public string Year { get; set; } // DB is char?
        public string Edition { get; set; }
        public string Publisher { get; set; }
        public int InStock { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal Subtotal { get; set; } // change to cost
        public void UpdateCost()
        {
            Subtotal = Price * Quantity;
        }

        //public DateTime CartTimestamp { get; set; } for 48 hour cart expiry

    }
}
