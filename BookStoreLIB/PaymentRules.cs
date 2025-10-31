using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace BookStoreLIB
{
    public static class PaymentRules
    {
        /// <summary>Returns the required keys whose values are null/empty/whitespace.</summary>
        public static List<string> GetMissingFields(Dictionary<string, string> requiredFields)
            => requiredFields.Where(kv => string.IsNullOrWhiteSpace(kv.Value))
                             .Select(kv => kv.Key)
                             .ToList();

        public static bool IsValidEmail(string email)
        {
            try
            {
                _ = new MailAddress(email ?? string.Empty);
                return true;
            }
            catch { return false; }
        }

        /// <summary>Card number must be 16 digits and pass Luhn.</summary>
        public static bool IsValidCardNumber(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber)) return false;
            var digits = new string(cardNumber.Where(char.IsDigit).ToArray());
            if (digits.Length != 16) return false;
            return PassesLuhn(digits);
        }

        public static bool IsValidCVV(string cvv) => !string.IsNullOrWhiteSpace(cvv) && cvv.All(char.IsDigit) && cvv.Length == 3;

        /// <summary>Expiry must be MM/YY, valid month 01..12, and not already expired relative to now.</summary>
        public static bool IsValidExpiry(string expiry, DateTime nowUtc)
        {
            if (string.IsNullOrWhiteSpace(expiry)) return false;
            // Accept "MM/YY" exactly.
            var parts = expiry.Split('/');
            if (parts.Length != 2) return false;
            if (!int.TryParse(parts[0], out int mm) || !int.TryParse(parts[1], out int yy2)) return false;
            if (mm < 1 || mm > 12) return false;

            // Convert YY -> 20YY (supports 00–99; adjust if you want a sliding window)
            int year = 2000 + yy2;
            // Expiry at end of month (common card rule).
            var lastDay = DateTime.DaysInMonth(year, mm);
            var expiryEndLocal = new DateTime(year, mm, lastDay, 23, 59, 59, DateTimeKind.Local);
            var nowLocal = nowUtc.ToLocalTime();

            return expiryEndLocal >= nowLocal;
        }

        /// <summary>Subtotal = sum(price * qty). Taxes rounded to 2 decimals. Total = subtotal + taxes + delivery.</summary>
        public static (decimal subtotal, decimal taxes, decimal total) ComputeTotals(IEnumerable<Book> items, decimal taxRate, decimal deliveryFee)
        {
            var subtotal = items?.Sum(b => b.Price * b.Quantity) ?? 0m;
            var taxes = Math.Round(subtotal * taxRate, 2, MidpointRounding.AwayFromZero);
            var total = subtotal + taxes + deliveryFee;
            return (subtotal, taxes, total);
        }

        private static bool PassesLuhn(string digits)
        {
            int sum = 0;
            bool dbl = false;
            for (int i = digits.Length - 1; i >= 0; i--)
            {
                int d = digits[i] - '0';
                if (dbl)
                {
                    d *= 2;
                    if (d > 9) d -= 9;
                }
                sum += d;
                dbl = !dbl;
            }
            return sum % 10 == 0;
        }
    }

    public static class PaymentSummaryBuilder
    {
        /// <summary>
        /// Builds the same summary your window shows, without UI dependencies.
        /// </summary>
        public static string BuildOrderSummary(
            IEnumerable<Book> items,
            decimal subtotal,
            decimal taxes,
            decimal deliveryFee,
            decimal total,
            DateTime now)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== ORDER CONFIRMED ===");
            sb.AppendLine($"Order Date: {now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine("----------------------------------------");
            sb.AppendLine("ITEMS ORDERED:");
            sb.AppendLine();

            foreach (var book in items ?? Enumerable.Empty<Book>())
            {
                var line = $"{book.Title}\t{book.Quantity}\t${(book.Price * book.Quantity):F2}";
                sb.AppendLine(line);
            }

            sb.AppendLine("----------------------------------------");
            sb.AppendLine($"SUBTOTAL:\t\t${subtotal:F2}");
            sb.AppendLine($"TAX (13%):\t\t${taxes:F2}");
            sb.AppendLine($"DELIVERY FEE:\t\t${deliveryFee:F2}");
            sb.AppendLine("----------------------------------------");
            sb.AppendLine($"TOTAL:\t\t\t${total:F2}");
            sb.AppendLine();
            sb.AppendLine("Thank you for your order!");
            sb.AppendLine("A confirmation email has been sent.");

            return sb.ToString();
        }
    }
}