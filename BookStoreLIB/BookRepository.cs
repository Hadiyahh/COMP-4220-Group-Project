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
            var raw = ConfigurationManager.ConnectionStrings["BookStoreRemote"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(raw))
                throw new InvalidOperationException("Missing connection string 'BookStoreRemote' in App.config.");

            _cs = Environment.ExpandEnvironmentVariables(raw);
        }

        public async Task<List<Book>> GetAllAsync()
        {
            var list = new List<Book>();

            using (var conn = new SqlConnection(_cs))
            {
                await conn.OpenAsync();

                const string sql = @"
SELECT ISBN, CategoryID, Title, Author, Price, Year, InStock
FROM dbo.BookData
ORDER BY Title;";

                using (var cmd = new SqlCommand(sql, conn))
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                    {
                        list.Add(new Book
                        {
                            ISBN = r.GetString(0).Trim(),                 // CHAR(10) padding
                            CategoryID = r.GetInt32(1),
                            Title = r.IsDBNull(2) ? "" : r.GetString(2),
                            Author = r.IsDBNull(3) ? "" : r.GetString(3),
                            Price = r.IsDBNull(4) ? 0m : r.GetDecimal(4),
                            Year = r.IsDBNull(5) ? "" : r.GetString(5).Trim(), // NCHAR(4)
                            InStock = r.GetInt32(6)
                        });
                    }
                }
            }

            return list;
        }
    }
}
