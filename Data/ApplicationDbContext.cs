using System.Data;
using Npgsql;

namespace EshopDapper.Data;

public class ApplicationDbContext
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public ApplicationDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = _configuration.GetConnectionString("Postgres");
    }

    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}