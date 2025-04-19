using Dapper;
using EshopDapper.Data;
using EshopDapper.DTO;
using EshopDapper.Entities;

namespace EshopDapper.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", async (ApplicationDbContext db) =>
        {
            const string sql = @"
                SELECT o.*, u.""Name"" as UserName, e.""Name"" as EmployeeName, s.name as ShipperName,
               od.""Id"" as OrderDetailId, od.""OrderId"", od.""ProductId"", od.""Price"", od.""Quantity"", od.""Created"",
               p.*
                FROM orders o
                LEFT JOIN users u ON o.""UserId"" = u.""Id""
                LEFT JOIN employees e ON o.""EmployeeId"" = e.""Id""
                LEFT JOIN shippers s ON o.""ShipperId"" = s.id
                LEFT JOIN orderdetails od ON o.""Id"" = od.""OrderId""
                LEFT JOIN products p ON od.""ProductId"" = p.""Id""
                ORDER BY o.""Id""";

            using var connection = db.CreateConnection();

            var orderDict = new Dictionary<int, OrderFull>();

            var result = await connection.QueryAsync<OrderFull, OrderItemFull, Product, OrderFull>(
                sql,
                (order, detail, product) =>
                {
                    if (!orderDict.TryGetValue(order.Id, out var currentOrder))
                    {
                        currentOrder = order;
                        currentOrder.OrderDetails = new List<OrderItemFull>();
                        orderDict.Add(order.Id, currentOrder);
                    }

                    if (detail != null && detail.Id != 0)
                    {
                        detail.Product = product;
                        currentOrder.OrderDetails.Add(detail);
                    }

                    return currentOrder;
                },
                splitOn: "OrderDetailId,Id"
            );

            return Results.Ok(orderDict.Values);
        });

        app.MapPost("/", async (ApplicationDbContext db, OrderCreateDto orderDto) =>
        {
            using var connection = db.CreateConnection();
            using var transaction = connection.BeginTransaction();
            var totalPrice = 0.0;
            foreach (var item in orderDto.Items)
            {
                totalPrice += item.Price;
            }

            try
            {
                const string insertOrderSql = @"
                    INSERT INTO orders 
                    (""UserId"", ""EmployeeId"", ""TotalPrice"", ""ShipCity"", ""ShipPostalCode"")
                    VALUES (@UserId, @EmployeeId, @TotalPrice, @ShipAddress, @ShipCity, @ShipPostalCode)
                    RETURNING ""Id"";";

                // Insert order and get the generated Id
                var orderId = await connection.ExecuteScalarAsync<int>
                (insertOrderSql,
                    new
                    {
                        orderDto.UserId, orderDto.EmployeeId, TotalPrice = totalPrice, orderDto.ShipCity,
                        orderDto.ShipPostalCode
                    }
                    , transaction);

                const string insertDetailsSql = @"
                    INSERT INTO orderdetails
                    (""OrderId"", ""ProductId"", ""Quantity"")
                    VALUES (@OrderId, @ProductId, @Quantity);";

                foreach (var item in orderDto.Items)
                {
                    await connection.ExecuteAsync(insertDetailsSql, new
                    {
                        OrderId = orderId,
                        item.ProductId,
                        item.Quantity,
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


        app.MapGet("/{id:int}", async (ApplicationDbContext db, int id) =>
        {
            const string sql = @"
                SELECT o.*, od.*, p.*
                FROM eshop.orders o
                LEFT JOIN eshop.orderdetails od ON o.""Id"" = od.""OrderId""
                LEFT JOIN eshop.products p ON od.""ProductId"" = p.""Id""
                WHERE o.""Id"" = @Id";

            using var connection = db.CreateConnection();

            var orderDict = new Dictionary<int, OrderFull>();

            var result = await connection.QueryAsync<OrderFull, OrderItemFull, Product, OrderFull>(
                sql,
                (order, detail, product) =>
                {
                    if (!orderDict.TryGetValue(order.Id, out var currentOrder))
                    {
                        currentOrder = order;
                        currentOrder.OrderDetails = new List<OrderItemFull>();
                        orderDict.Add(order.Id, currentOrder);
                    }

                    if (detail != null)
                    {
                        detail.Product = product;
                        currentOrder.OrderDetails.Add(detail);
                    }

                    return currentOrder;
                },
                new { Id = id },
                splitOn: "Id,Id"
            );

            var finalOrder = result.FirstOrDefault();
            return finalOrder is null ? Results.NotFound() : Results.Ok(finalOrder);
        });

        app.MapDelete("/{id:int}", async (ApplicationDbContext db, int id) =>
        {
            const string sql = "DELETE FROM orders WHERE \"Id\" = @Id";
            using var connection = db.CreateConnection();

            var result = await connection.ExecuteAsync(sql, new { Id = id });

            return result == 0 ? Results.NotFound() : Results.Ok(result);
        });
    }
}

/*app.MapPut("/orders/{id:int}", async (int id, OrderCreateDto request, ApplicationDbContext db) =>
{
    using var connection = db.CreateConnection();
    using var tx = connection.BeginTransaction();

    try
    {
// 1. Vrati stare stavke i vrati količine u products
var oldItems = await connection.QueryAsync<OrderItemDto>(
    @"SELECT ""ProductId"", ""Quantity"", ""Price""
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
foreach (var item in request.Items)
{
    await connection.ExecuteAsync(
        @"INSERT INTO eshop.orderdetails
          (""OrderId"", ""ProductId"", ""Quantity"")
          VALUES (@OrderId, @ProductId, @Quantity);",
        new
        {
            OrderId = id,
            item.ProductId,
            item.Quantity
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
});*/

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