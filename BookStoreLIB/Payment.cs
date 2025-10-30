using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;


namespace BookStoreGUI
{
    public partial class Payment
    {
        public static (bool, string) Validate(string name, string number, string expiry, string cvv, string email)
        {
            if (string.IsNullOrWhiteSpace(name) ||
               string.IsNullOrWhiteSpace(number) ||
               string.IsNullOrWhiteSpace(expiry) ||
               string.IsNullOrWhiteSpace(cvv) ||
               string.IsNullOrWhiteSpace(email))
                return (false, "Please fill in all fields.");

            if (cvv.Length != 3 || number.Length != 16 || !expiry.Contains("/"))
                return (false, "Please check card details.");

            try { _ = new MailAddress(email); }
            catch { return (false, "Invalid email."); }

            return (true, string.Empty);
        }
    }
}
