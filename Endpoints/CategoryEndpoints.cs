using Dapper;
using EshopDapper.Data;
using EshopDapper.DTO;
using EshopDapper.Entities;

namespace EshopDapper.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/",async (ApplicationDbContext db) =>
        {
            const string sql = @"SELECT *
                FROM eshop.categories";
            using var connection = db.CreateConnection();
            
            var categories = await connection.QueryAsync<List<Category>>(sql);
            
            return Results.Ok(categories);
        });
        
        app.MapPost("/", async (ApplicationDbContext db, CategoryPost categorydto) =>
        {
            const string sql = "INSERT INTO categories (\"Name\", \"CreatedBy\") VALUES (@Name, @CreatedBy)";
            using var connection = db.CreateConnection();
            var result = await connection.ExecuteAsync(sql, categorydto);
            
            return Results.Ok(result);
        });
        
        app.MapGet("/{id:int}", async (ApplicationDbContext db, int id) =>
        {
            const string sql = @"SELECT * FROM categories WHERE ""Id""= @Id";
            using var connection = db.CreateConnection();
            
            var category = await connection.QuerySingleOrDefaultAsync<Category>(sql, new { Id = id });
            
            return category is null ? Results.NotFound() : Results.Ok(category);
        });
        
        app.MapPut("/{id:int}", async (ApplicationDbContext db, int id, CategoryPost categorydto) =>
        {
            const string sql = "UPDATE categories SET \"Name\" = @Name, \"CreatedBy\" = @CreatedBy WHERE \"Id\" = @Id";
            using var connection = db.CreateConnection();
            
            var result = await connection.ExecuteAsync(sql, new { categorydto.Name, categorydto.CreatedBy, Id = id });
            
            return result == 0 ? Results.NotFound() : Results.Ok(result);
        });
        
        app.MapDelete("/{id:int}", async (ApplicationDbContext db, int id) =>
        {
            const string sql = "DELETE FROM categories WHERE \"Id\" = @Id";
            using var connection = db.CreateConnection();
            
            var result = await connection.ExecuteAsync(sql, new { Id = id });
            
            return result == 0 ? Results.NotFound() : Results.Ok(result);
        });
    }
}