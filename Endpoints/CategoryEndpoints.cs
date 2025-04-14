using Dapper;
using EshopDapper.Data;
using EshopDapper.DTO;
using EshopDapper.Entities;
using Mapster;

namespace EshopDapper.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/categories",async (ApplicationDbContext db) =>
        {
            const string sql = "SELECT * FROM categories";
            using var connection = db.CreateConnection();
            
            var categories = await connection.QueryAsync<Category>(sql);
            
            return Results.Ok(categories);
        });
        
        app.MapPost("/categories", async (ApplicationDbContext db, CategoryPost categorydto) =>
        {
            const string sql = "INSERT INTO categories (\"Name\", \"CreatedBy\") VALUES (@Name, @CreatedBy)";

            using var connection = db.CreateConnection();
            
            // var categoryPost = categorydto.Adapt<Category>(); 
            
            var result = await connection.ExecuteAsync(sql, categorydto);
            
            return Results.Ok(result);
        });
        
        app.MapGet("/categories/{id:int}", async (ApplicationDbContext db, int id) =>
        {
            const string sql = "SELECT * FROM categories WHERE Id= @Id";
            using var connection = db.CreateConnection();
            
            var category = await connection.QuerySingleOrDefaultAsync<Category>(sql, new { Id = id });
            
            return category is null ? Results.NotFound() : Results.Ok(category);
        });
        
        app.MapPut("/categories/{id:int}", async (ApplicationDbContext db, int id, CategoryPost categorydto) =>
        {
            const string sql = "UPDATE categories SET \"Name\" = @Name, \"CreatedBy\" = @CreatedBy WHERE \"Id\" = @Id";
            using var connection = db.CreateConnection();
            
            var result = await connection.ExecuteAsync(sql, new { Name = categorydto.Name, CreatedBy = categorydto.CreatedBy, Id = id });
            
            return result == 0 ? Results.NotFound() : Results.Ok(result);
        });
        
        app.MapDelete("/categories/{id:int}", async (ApplicationDbContext db, int id) =>
        {
            const string sql = "DELETE FROM categories WHERE \"Id\" = @Id";
            using var connection = db.CreateConnection();
            
            var result = await connection.ExecuteAsync(sql, new { Id = id });
            
            return result == 0 ? Results.NotFound() : Results.Ok(result);
        });
    }
}