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
            const string sql = @"
        SELECT
            o.""Id"" AS OrderId,
            o.""UserId"",
            o.""EmployeeId"",
            o.""TotalPrice"",
            o.""OrderDate"",
            o.""ShippedDate"",
            o.""ShipCity"",
            o.""ShipPostalCode"",
            
            od.""ProductId"",
            od.""Price"" AS OrderPrice,
            od.""Quantity"",
            od.""Discount"",
       
            p.""Name"" AS ProductName
        FROM eshop.orders o
        JOIN eshop.orderdetails od ON o.""Id"" = od.""OrderId""
        JOIN eshop.products p ON od.""ProductId"" = p.""Id""
        ORDER BY o.""Id""";

            using var connection = db.CreateConnection();

            var orderLookup = new Dictionary<int, OrderResultDto>();

            var result = await connection.QueryAsync<OrderResultDto, OrderItemResult, OrderResultDto>(
                sql,
                (order, item) =>
                {
                    if (!orderLookup.TryGetValue(order.OrderId, out var existingOrder))
                    {
                        existingOrder = order;
                        existingOrder.Items = new List<OrderItemResult>();
                        orderLookup[order.OrderId] = existingOrder;
                    }

                    existingOrder.Items.Add(item);
                    return existingOrder;
                },
                splitOn: "ProductId"
            );

            return Results.Ok(orderLookup.Values);
        });
        
        app.MapGet("/orders/{id:int}", async (int id, ApplicationDbContext db) =>
        {
            const string sql = @"
        SELECT
            o.""Id"" AS OrderId,
            o.""UserId"",
            o.""EmployeeId"",
            o.""TotalPrice"",
            o.""OrderDate"",
            o.""ShippedDate"",
            o.""ShipCity"",
            o.""ShipPostalCode"",
            
            od.""ProductId"",
            od.""Price"" AS OrderPrice,
            od.""Quantity"",
            od.""Discount"",
            
            p.""Name"" AS ProductName
        FROM eshop.orders o
        JOIN eshop.orderdetails od ON o.""Id"" = od.""OrderId""
        JOIN eshop.products p ON od.""ProductId"" = p.""Id""
        WHERE o.""Id"" = @Id;";

            using var connection = db.CreateConnection();

            var orderLookup = new Dictionary<int, OrderResultDto>();

            var result = await connection.QueryAsync<OrderResultDto, OrderItemResult, OrderResultDto>(
                sql,
                (order, item) =>
                {
                    if (!orderLookup.TryGetValue(order.OrderId, out var existingOrder))
                    {
                        existingOrder = order;
                        existingOrder.Items = new List<OrderItemResult>();
                        orderLookup[order.OrderId] = existingOrder;
                    }

                    existingOrder.Items.Add(item);
                    return existingOrder;
                },
                new { Id = id },
                splitOn: "ProductId"
            );

            var finalOrder = orderLookup.Values.FirstOrDefault();
            return finalOrder is not null ? Results.Ok(finalOrder) : Results.NotFound();
        });


        app.MapPost("/orders", async (ApplicationDbContext db, OrderCreateDto orderDto) =>
        {
            using var connection = db.CreateConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                const string insertOrderSql = @"
                    INSERT INTO eshop.orders 
                    (""UserId"", ""EmployeeId"", ""TotalPrice"", ""OrderDate"", ""ShipCity"", ""ShipPostalCode"", ""Created"")
                    VALUES (@UserId, @EmployeeId, @TotalPrice, @OrderDate, @ShipCity, @ShipPostalCode, now())
                    RETURNING ""Id"";";

                // Insert order and get the generated Id
                var orderId = await connection.ExecuteScalarAsync<int>(insertOrderSql, orderDto, transaction);

                const string insertDetailsSql = @"
                    INSERT INTO eshop.orderdetails
                    (""OrderId"", ""ProductId"", ""Price"", ""Quantity"", ""Discount"", ""Created"")
                    VALUES (@OrderId, @ProductId, @Price, @Quantity, @Discount, now());";

                foreach (var item in orderDto.Items)
                {
                    await connection.ExecuteAsync(insertDetailsSql, new
                    {
                        OrderId = orderId,
                        ProductId = item.ProductId,
                        Price = item.Price,
                        Quantity = item.Quantity,
                        Discount = item.Discount
                    }, transaction);
                }

                transaction.Commit();
                return Results.Created($"/orders/{orderId}", new { OrderId = orderId });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return Results.Problem(ex.Message);
            }
        });


        app.MapGet("/orders/{id:int}", async (ApplicationDbContext db, int id) =>
        {
            const string sql = "SELECT * FROM orders WHERE Id= @Id";
            using var connection = db.CreateConnection();

            var order = await connection.QuerySingleOrDefaultAsync<Order>(sql, new { Id = id });

            return order is null ? Results.NotFound() : Results.Ok(order);
        });

        app.MapPut("/orders/{id:int}", async (int id, CreateOrderRequest request, ApplicationDbContext db) =>
        {   
            using var connection = db.CreateConnection();
            using var tx = connection.BeginTransaction();

    try
    {
        // 1. Vrati stare stavke i vrati količine u products
        var oldItems = await connection.QueryAsync<OrderItemDto>(
            @"SELECT ""ProductId"", ""Quantity"", ""Price"", ""Discount""
              FROM eshop.orderdetails
              WHERE ""OrderId"" = @OrderId;",
            new { OrderId = id }, tx);

        foreach (var item in oldItems)
        {
            await connection.ExecuteAsync(
                @"UPDATE eshop.products
                  SET ""Amount"" = ""Amount"" + @Quantity
                  WHERE ""Id"" = @ProductId;",
                new { item.ProductId, item.Quantity }, tx);
        }

        // 2. Obriši stare stavke
        await connection.ExecuteAsync(
            @"DELETE FROM eshop.orderdetails WHERE ""OrderId"" = @OrderId;",
            new { OrderId = id }, tx);

        // 3. Dodaj nove stavke i skini količinu
        foreach (var item in request.OrderItems)
        {
            await connection.ExecuteAsync(
                @"INSERT INTO eshop.orderdetails
                  (""OrderId"", ""ProductId"", ""Price"", ""Quantity"", ""Discount"", ""Created"")
                  VALUES (@OrderId, @ProductId, @Price, @Quantity, @Discount, NOW());",
                new
                {
                    OrderId = id,
                    item.ProductId,
                    item.Price,
                    item.Quantity,
                    item.Discount
                }, tx);

            await connection.ExecuteAsync(
                @"UPDATE eshop.products
                  SET ""Amount"" = ""Amount"" - @Quantity
                  WHERE ""Id"" = @ProductId AND ""Amount"" >= @Quantity;",
                new { item.ProductId, item.Quantity }, tx);
        }

        // 4. Ažuriraj glavnu porudžbinu
        var total = request.OrderItems.Sum(x => x.Price * x.Quantity - (x.Discount ?? 0));
        await connection.ExecuteAsync(
            @"UPDATE eshop.orders
              SET ""UserId"" = @UserId,
                  ""EmployeeId"" = @EmployeeId,
                  ""ShipCity"" = @ShipCity,
                  ""ShipPostalCode"" = @ShipPostalCode,
                  ""TotalPrice"" = @TotalPrice,
                  ""Updated"" = NOW()
              WHERE ""Id"" = @OrderId;",
            new
            {
                OrderId = id,
                request.UserId,
                request.EmployeeId,
                request.ShipCity,
                request.ShipPostalCode,
                TotalPrice = total
            }, tx);

        tx.Commit();
        return Results.NoContent(); // ili Results.Ok() ako želiš da vratiš novu verziju
    }
    catch (Exception ex)
    {
        tx.Rollback();
        return Results.BadRequest(new { error = ex.Message });
    }
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

// ENDPOINTS 
/*app.MapPost("/orders", async (OrderCreateDto orderDto, EshopDbContext db) =>
{
    var order = new Order
    {
        UserId = orderDto.UserId,
        EmployeeId = orderDto.EmployeeId,
        TotalPrice = orderDto.TotalPrice,
        OrderDate = orderDto.OrderDate,
        ShipCity = orderDto.ShipCity,
        ShipPostalCode = orderDto.ShipPostalCode,
        Created = DateTime.UtcNow,
        OrderDetails = orderDto.Items.Select(item => new OrderDetail
        {
            ProductId = item.ProductId,
            Price = item.Price,
            Quantity = item.Quantity,
            Discount = item.Discount,
            Created = DateTime.UtcNow
        }).ToList()
    };

    db.Orders.Add(order);
    await db.SaveChangesAsync();

    return Results.Created($"/orders/{order.Id}", new { order.Id });
});*/

/*app.MapGet("/orders", async (EshopDbContext db) =>
{
    var orders = await db.Orders
        .Include(o => o.OrderDetails)
        .ThenInclude(od => od.Product)
        .Include(o => o.User) // ako imaš navigaciju na User
        .ToListAsync();

    var result = orders.Select(o => new
    {
        o.Id,
        o.UserId,
        o.EmployeeId,
        o.TotalPrice,
        o.OrderDate,
        o.ShippedDate,
        o.ShipCity,
        o.ShipPostalCode,
        Items = o.OrderDetails.Select(d => new
        {
            d.ProductId,
            ProductName = d.Product?.Name,
            d.Price,
            d.Quantity,
            d.Discount
        })
    });

    return Results.Ok(result);
});*/