using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace BookStoreLIB
{
    public sealed class BookRepository
    {
        private readonly string _cs;

        public BookRepository()
        {
            // Try both keys so GUI/config differences don't break the repo.
            var csEntry =
                ConfigurationManager.ConnectionStrings["BookStoreRemote"] ??
                ConfigurationManager.ConnectionStrings["BookStoreDBConnectionString"];

            var raw = csEntry?.ConnectionString;
            if (string.IsNullOrWhiteSpace(raw))
            {
                throw new InvalidOperationException(
                    "Missing connection string. Define 'BookStoreRemote' or 'BookStoreDBConnectionString' in the startup project's App.config.");
            }

            _cs = Environment.ExpandEnvironmentVariables(raw);
        }

        public async Task<List<Book>> GetAllAsync()
        {
            var list = new List<Book>();

            try
            {
                using (var conn = new SqlConnection(_cs))
                {
                    await conn.OpenAsync().ConfigureAwait(false);

                    const string sql = @"
SELECT ISBN, CategoryID, Title, Author, Price, Year, InStock
FROM dbo.BookData
ORDER BY Title;";

                    using (var cmd = new SqlCommand(sql, conn))
                    using (var r = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await r.ReadAsync().ConfigureAwait(false))
                        {
                            list.Add(new Book
                            {
                                // Adjust these getters if your schema types differ.
                                ISBN = r.GetString(0).Trim(),                 // CHAR(10) often padded
                                CategoryID = r.GetInt32(1),
                                Title = r.IsDBNull(2) ? "" : r.GetString(2),
                                Author = r.IsDBNull(3) ? "" : r.GetString(3),
                                Price = r.IsDBNull(4) ? 0m : r.GetDecimal(4), // MONEY/DECIMAL
                                Year = r.IsDBNull(5) ? "" : r.GetString(5).Trim(), // (N)CHAR(4)
                                InStock = r.GetInt32(6)
                            });
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Surface a helpful message upstream; caller can show it.
                throw new InvalidOperationException(
                    "Failed to fetch books from database. Check connection string, credentials, and table/column names.",
                    ex);
            }

            return list;
        }
    }
}
