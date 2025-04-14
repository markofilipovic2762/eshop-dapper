using Dapper;
using EshopDapper.Data;
using EshopDapper.DTO;
using EshopDapper.Entities;

namespace EshopDapper.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/orders", async (ApplicationDbContext db) =>
        {
            const string sql = "SELECT * FROM orders";
            using var connection = db.CreateConnection();

            var orders = await connection.QueryAsync<Order>(sql);

            return Results.Ok(orders);
        });

        app.MapPost("/orders", async (ApplicationDbContext db, OrderPost orderdto) =>
        {
            const string sql = "INSERT INTO orders (\"ProductId\", \"Quantity\", \"CreatedBy\") VALUES (@ProductId, @Quantity, @CreatedBy)";

            using var connection = db.CreateConnection();

            var result = await connection.ExecuteAsync(sql, orderdto);

            return Results.Ok(result);
        });

        app.MapGet("/orders/{id:int}", async (ApplicationDbContext db, int id) =>
        {
            const string sql = "SELECT * FROM orders WHERE Id= @Id";
            using var connection = db.CreateConnection();

            var order = await connection.QuerySingleOrDefaultAsync<Order>(sql, new { Id = id });

            return order is null ? Results.NotFound() : Results.Ok(order);
        });

        app.MapPut("/orders/{id:int}", async (ApplicationDbContext db, int id, OrderPost orderdto) =>
        {
            const string sql = "UPDATE orders SET \"ProductId\" = @ProductId, \"Quantity\" = @Quantity, \"CreatedBy\" = @CreatedBy WHERE \"Id\" = @Id";
            using var connection = db.CreateConnection();

            var result = await connection.ExecuteAsync(sql, new { ProductId = orderdto.ProductId, Quantity = orderdto.Quantity, CreatedBy = orderdto.CreatedBy, Id = id });

            return result == 0 ? Results.NotFound() : Results.Ok(result);
        });

        app.MapDelete("/orders/{id:int}", async (ApplicationDbContext db, int id) =>
        {
            const string sql = "DELETE FROM orders WHERE \"Id\" = @Id";
            using var connection = db.CreateConnection();

            var result = await connection.ExecuteAsync(sql, new { Id = id });

            return result == 0 ? Results.NotFound() : Results.Ok(result);
        });
    }
}
