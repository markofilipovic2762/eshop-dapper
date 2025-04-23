using Dapper;
using EshopDapper.Data;
using EshopDapper.DTO.Supplier;
using EshopDapper.Entities;

namespace EshopDapper.Endpoints;

public static class SupplierEndpoints
{
    public static void MapSupplierEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", async (ApplicationDbContext db) =>
        {
            const string sql = "SELECT * FROM suppliers";
            using var connection = db.CreateConnection();
            
            var suppliers = await connection.QueryAsync<Supplier>(sql);
            
            return Results.Ok(suppliers);
        });
        
        app.MapPost("/", async (ApplicationDbContext db, SupplierPostRequest supplier) =>
        {
            const string sql = @"INSERT INTO suppliers 
                (""Name"",""Phone"", ""Email"", ""Address"", ""City"") 
                VALUES (@Name, @Phone, @Email, @Address, @City)";
            
            using var connection = db.CreateConnection();
            var result = await connection.ExecuteAsync(sql, supplier);
            
            return Results.Ok(result);
        });
        
        app.MapGet("/{id:int}", async (ApplicationDbContext db, int id) =>
        {
            const string sql = "SELECT * FROM suppliers WHERE \"Id\" = @Id";
            using var connection = db.CreateConnection();
            
            var supplier = await connection.QuerySingleOrDefaultAsync<Supplier>(sql, new { Id = id });
            
            return supplier is null ? Results.NotFound() : Results.Ok(supplier);
        });
        
        app.MapPut("/{id:int}", async (ApplicationDbContext db, int id, SupplierPostRequest supplier) =>
        {
            const string sql = @"UPDATE suppliers 
                SET ""Name"" = @Name, 
                    ""Phone"" = @Phone, 
                    ""Email"" = @Email, 
                    ""Address"" = @Address, 
                    ""City"" = @City 
                WHERE ""Id"" = @Id";
            
            using var connection = db.CreateConnection();
            var result = await connection.ExecuteAsync(sql, new
            {
                supplier.Name,
                supplier.Phone,
                supplier.Email,
                supplier.Address,
                supplier.City,
                Id = id
            });
            
            return result == 0 ? Results.NotFound() : Results.Ok(result);
        });
        
        app.MapDelete("/{id:int}", async (ApplicationDbContext db, int id) =>
        {
            const string sql = "DELETE FROM suppliers WHERE \"Id\" = @Id";
            using var connection = db.CreateConnection();
            
            var result = await connection.ExecuteAsync(sql, new { Id = id });
            
            return result == 0 ? Results.NotFound() : Results.Ok(result);
        });
    }
}