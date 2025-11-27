using Microsoft.Data.SqlClient;

namespace MineSweeper_MVC.Data
{
    public class DatabaseContext
    {
        private readonly string _connectionString;

        public DatabaseContext(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
