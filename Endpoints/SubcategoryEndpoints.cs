using Dapper;
using EshopDapper.Data;
using EshopDapper.DTO.Subcategory;
using EshopDapper.Entities;

namespace EshopDapper.Endpoints;

public static class SubcategoryEndpoints
{
    public static void MapSubcategoryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/subcategories", async (ApplicationDbContext db) =>
        {
            const string sql = "SELECT * FROM subcategories";
            using var connection = db.CreateConnection();

            var subcategories = await connection.QueryAsync<Subcategory>(sql);

            return Results.Ok(subcategories);
        });

        app.MapPost("/subcategories", async (ApplicationDbContext db, SubcategoryPost subcategorydto) =>
        {
            const string sql = "INSERT INTO subcategories (\"Name\", \"CategoryId\", \"CreatedBy\") VALUES (@Name, @CategoryId, @CreatedBy)";

            using var connection = db.CreateConnection();

            var result = await connection.ExecuteAsync(sql, subcategorydto);

            return Results.Ok(result);
        });

        app.MapGet("/subcategories/{id:int}", async (ApplicationDbContext db, int id) =>
        {
            const string sql = "SELECT * FROM subcategories WHERE Id= @Id";
            using var connection = db.CreateConnection();

            var subcategory = await connection.QuerySingleOrDefaultAsync<Subcategory>(sql, new { Id = id });

            return subcategory is null ? Results.NotFound() : Results.Ok(subcategory);
        });

        app.MapPut("/subcategories/{id:int}", async (ApplicationDbContext db, int id, SubcategoryPost subcategorydto) =>
        {
            const string sql = "UPDATE subcategories SET \"Name\" = @Name, \"CategoryId\" = @CategoryId, \"CreatedBy\" = @CreatedBy WHERE \"Id\" = @Id";
            using var connection = db.CreateConnection();

            var result = await connection.ExecuteAsync(sql, new { Name = subcategorydto.Name, CategoryId = subcategorydto.CategoryId, CreatedBy = subcategorydto.CreatedBy, Id = id });

            return result == 0 ? Results.NotFound() : Results.Ok(result);
        });

        app.MapDelete("/subcategories/{id:int}", async (ApplicationDbContext db, int id) =>
        {
            const string sql = "DELETE FROM subcategories WHERE \"Id\" = @Id";
            using var connection = db.CreateConnection();

            var result = await connection.ExecuteAsync(sql, new { Id = id });

            return result == 0 ? Results.NotFound() : Results.Ok(result);
        });
    }
}