using Dapper;
using EshopDapper.Data;
using EshopDapper.DTO;
using EshopDapper.Entities;

namespace EshopDapper.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/products", async (ApplicationDbContext db) =>
        {
            const string sql = "SELECT * FROM products";
            using var connection = db.CreateConnection();

            var products = await connection.QueryAsync<Product>(sql);

            return Results.Ok(products);
        });

        app.MapPost("/products", async (ApplicationDbContext db, ProductPost productdto) =>
        {
            const string sql = "INSERT INTO products (\"Name\", \"Price\", \"CreatedBy\") VALUES (@Name, @Price, @CreatedBy)";

            using var connection = db.CreateConnection();

            var result = await connection.ExecuteAsync(sql, productdto);

            return Results.Ok(result);
        });

        app.MapGet("/products/{id:int}", async (ApplicationDbContext db, int id) =>
        {
            const string sql = "SELECT * FROM products WHERE Id= @Id";
            using var connection = db.CreateConnection();

            var product = await connection.QuerySingleOrDefaultAsync<Product>(sql, new { Id = id });

            return product is null ? Results.NotFound() : Results.Ok(product);
        });

        app.MapPut("/products/{id:int}", async (ApplicationDbContext db, int id, ProductPost productdto) =>
        {
            const string sql = "UPDATE products SET \"Name\" = @Name, \"Price\" = @Price, \"CreatedBy\" = @CreatedBy WHERE \"Id\" = @Id";
            using var connection = db.CreateConnection();

            var result = await connection.ExecuteAsync(sql, new { Name = productdto.Name, Price = productdto.Price, CreatedBy = productdto.CreatedBy, Id = id });

            return result == 0 ? Results.NotFound() : Results.Ok(result);
        });

        app.MapDelete("/products/{id:int}", async (ApplicationDbContext db, int id) =>
        {
            const string sql = "DELETE FROM products WHERE \"Id\" = @Id";
            using var connection = db.CreateConnection();

            var result = await connection.ExecuteAsync(sql, new { Id = id });

            return result == 0 ? Results.NotFound() : Results.Ok(result);
        });
    }
}