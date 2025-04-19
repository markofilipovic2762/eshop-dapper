using Dapper;
using EshopDapper.Data;
using EshopDapper.DTO;
using EshopDapper.Entities;

namespace EshopDapper.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", async (ApplicationDbContext db) =>
        {
            const string sql = "SELECT * FROM products";
            using var connection = db.CreateConnection();

            var products = await connection.QueryAsync<Product>(sql);

            return Results.Ok(products);
        });

        app.MapPost("/", async (ApplicationDbContext db, ProductPost productdto) =>
        {
            const string sql = @"INSERT INTO products 
                (""Name"",""Description"", ""Price"",""Amount"",""ImageUrl"", ""CreatedBy"",""LastModified"",""LastModifiedBy"", ""CategoryId"", ""SubcategoryId"",""SupplierId"")
                VALUES 
                (@Name,@Description ,@Price,@Amount,@ImageUrl,@CreatedBy,@LastModified,@LastModifiedBy,@CategoryId,@SubcategoryId,@SupplierId)";

            using var connection = db.CreateConnection();

            var result = await connection.ExecuteAsync(sql, productdto);

            return Results.Ok(result);
        });

        app.MapGet("/{id:int}", async (ApplicationDbContext db, int id) =>
        {
            const string sql = @"SELECT * FROM products WHERE ""Id""= @Id";
            using var connection = db.CreateConnection();

            var product = await connection.QuerySingleOrDefaultAsync<Product>(sql, new { Id = id });

            return product is null ? Results.NotFound() : Results.Ok(product);
        });

        app.MapPut("/{id:int}", async (ApplicationDbContext db, int id, ProductPost productdto) =>
        {
            const string sql = @"UPDATE products 
                SET ""Name"" = @Name,""Description"" = @Description,""Price"" = @Price,""Amount""= @Amount,
                    ""ImageUrl"" = @ImageUrl,""CreatedBy""= @CreatedBy,""LastModified"" = now(),
                    ""LastModifiedBy"" = @LastModifiedBy,""CategoryId"" = @CategoryId, ""SubcategoryId"" = @SubcategoryId,
                    ""SupplierId"" = @SupplierId
                WHERE ""Id"" = @Id";
            using var connection = db.CreateConnection();

            var result = await connection.ExecuteAsync(sql, new
            {
                productdto.Name,
                productdto.Description,
                productdto.Price,
                productdto.Amount,
                productdto.ImageUrl,
                productdto.CreatedBy,
                productdto.LastModifiedBy,
                productdto.CategoryId,
                productdto.SubcategoryId,
                productdto.SupplierId,
                Id = id
            });

            return result == 0 ? Results.NotFound() : Results.Ok(result);
        });

        app.MapDelete("/{id:int}", async (ApplicationDbContext db, int id) =>
        {
            const string sql = "DELETE FROM products WHERE \"Id\" = @Id";
            using var connection = db.CreateConnection();

            var result = await connection.ExecuteAsync(sql, new { Id = id });

            return result == 0 ? Results.NotFound() : Results.Ok(result);
        });
    }
}