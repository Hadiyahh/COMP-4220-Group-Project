using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStoreLIB
{
    public class Cart // all cart related actions
    {
        public List<Book> shoppingCart = new List<Book>(); // list of books for each customer cart
        public bool addBook(Book book) // based on obj from DB booklist
        {
            for (int i = 0; i < shoppingCart.Count; i++)
            {
                if (shoppingCart[i].BookID == book.BookID)
                {
                    shoppingCart[i].Quantity += 1;
                    return true;
                }
            }
                //book.CartTimestamp = DateTime.Now;
                shoppingCart.Add(book);
                return true;
        }
        public bool removeBook(Book book) // removing cart books
        {
           
            for (int i = 0; i < shoppingCart.Count; i++)
            {
                if (shoppingCart[i].BookID == book.BookID)
                {
                    if (shoppingCart[i].Quantity > 1)
                    {
                        shoppingCart[i].Quantity -= 1;
                    }
                    else
                    {
                        shoppingCart.RemoveAt(i);
                        
                    }
                    return true;
                }    
                
            }

            return false;

        }

       /* public void ExpiredBooks()
        {
            DateTime now = DateTime.Now;

            for (int i = shoppingCart.Count -1; i>=0; i--)
            {
                DateTime added = shoppingCart[i].CartTimestamp;
                TimeSpan addedTime = DateTime.Now - added;
                if (addedTime.TotalHours >=48)
                {
                    shoppingCart.RemoveAt(i);
                }
            }
        } */
        public void clearCart() // clearing cart
        {
                shoppingCart.Clear();
           
        }
    }
}
