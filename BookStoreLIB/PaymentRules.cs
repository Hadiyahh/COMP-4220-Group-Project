using System;
using System.Collections.Generic;
using System.Net.Mail;

namespace BookStoreLIB
{
    public static class PaymentRules
    {
        // Go through each required field and collect the ones that are blank.
        public static List<string> GetMissingFields(Dictionary<string, string> requiredFields)
        {
            var missing = new List<string>();

            if (requiredFields == null)
            {
                return missing; // nothing to check
            }

            foreach (var pair in requiredFields)
            {
                string key = pair.Key;
                string value = pair.Value;

                if (value == null) value = "";
                if (value.Trim().Length == 0)
                {
                    missing.Add(key);
                }
            }

            return missing;
        }

        // Check if an email looks valid by trying to create a MailAddress.
        public static bool IsValidEmail(string email)
        {
            try
            {
                if (email == null) email = "";
                var addr = new MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Card number must be 16 digits and pass the Luhn check.
        public static bool IsValidCardNumber(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
                return false;

            // Keep only digits.
            string onlyDigits = "";
            foreach (char c in cardNumber)
            {
                if (char.IsDigit(c))
                {
                    onlyDigits += c;
                }
            }

            if (onlyDigits.Length != 16)
                return false;

            return PassesLuhn(onlyDigits);
        }

        // CVV is exactly 3 digits.
        public static bool IsValidCVV(string cvv)
        {
            if (string.IsNullOrWhiteSpace(cvv))
                return false;

            if (cvv.Length != 3)
                return false;

            foreach (char c in cvv)
            {
                if (!char.IsDigit(c))
                    return false;
            }

            return true;
        }

        // Expiry must be "MM/YY" and not in the past.
        public static bool IsValidExpiry(string expiry, DateTime nowUtc)
        {
            if (string.IsNullOrWhiteSpace(expiry))
                return false;

            string[] parts = expiry.Split('/');
            if (parts.Length != 2)
                return false;

            int month;
            int yy;
            if (!int.TryParse(parts[0], out month)) return false;
            if (!int.TryParse(parts[1], out yy)) return false;

            if (month < 1 || month > 12)
                return false;

            int year = 2000 + yy;

            // Last second of the expiry month in local time.
            int lastDay = DateTime.DaysInMonth(year, month);
            var expiryEndLocal = new DateTime(year, month, lastDay, 23, 59, 59, DateTimeKind.Local);
            var nowLocal = nowUtc.ToLocalTime();

            return expiryEndLocal >= nowLocal;
        }

        // Compute subtotal, taxes (rounded), and total.
        public static (decimal subtotal, decimal taxes, decimal total) ComputeTotals(
            IEnumerable<Book> items,
            decimal taxRate,
            decimal deliveryFee)
        {
            decimal subtotal = 0m;

            if (items != null)
            {
                foreach (var b in items)
                {
                    // Protect against null book entries just in case
                    if (b != null)
                    {
                        subtotal += (b.Price * b.Quantity);
                    }
                }
            }

            decimal taxes = Math.Round(subtotal * taxRate, 2, MidpointRounding.AwayFromZero);
            decimal total = subtotal + taxes + deliveryFee;

            return (subtotal, taxes, total);
        }

        // Standard Luhn algorithm.
        private static bool PassesLuhn(string digits)
        {
            int sum = 0;
            bool doubleNext = false;

            for (int i = digits.Length - 1; i >= 0; i--)
            {
                int d = digits[i] - '0';

                if (doubleNext)
                {
                    d = d * 2;
                    if (d > 9)
                    {
                        d = d - 9;
                    }
                }

                sum += d;
                doubleNext = !doubleNext;
            }

            return (sum % 10) == 0;
        }
    }
}
