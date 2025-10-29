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
        public List<Book> cartBooks = new List<Book>();
        public bool addBook(Book book) // based on obj from DB booklist
        {
            for (int i = 0; i < cartBooks.Count; i++)
            {
                if (cartBooks[i].ISBN == book.ISBN)
                {
                    cartBooks[i].Quantity += 1;
                    cartBooks[i].UpdateCost();
                    return true;
                }
            }
                    book.Quantity = 1;
                    cartBooks.Add(book);
                    book.UpdateCost();
                    return true;
                //book.CartTimestamp = DateTime.Now;
        }
        public bool removeBook(Book book) // removing cart books
        {
           
            for (int i = 0; i < cartBooks.Count; i++)
            {
                if (cartBooks[i].ISBN == book.ISBN)
                {
                    if (cartBooks[i].Quantity > 1)
                    {
                        cartBooks[i].Quantity -= 1;
                        cartBooks[i].UpdateCost();
                    }
                    else
                    {
                        cartBooks.RemoveAt(i);
                        
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
                cartBooks.Clear();
           
        }
    }
}
