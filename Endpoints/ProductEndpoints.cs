using Dapper;
using EshopDapper.Data;
using EshopDapper.DTO;
using EshopDapper.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EshopDapper.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", 
            async (ApplicationDbContext db, [FromQuery(Name = "category")] int[]? categoryIds,
                [FromQuery(Name = "subcategory")] int[]? subcategoryIds, int? supplierId, string? productName) =>
        {
            var sql = @"
            SELECT 
                p.""Id"", p.""Name"", p.""Description"", p.""Price"", p.""Amount"", p.""Sold"",
                p.""IsDeleted"", p.""CategoryId"", p.""SubcategoryId"", p.""SupplierId"",
                p.""Created"", p.""CreatedBy"", p.""LastModified"", p.""LastModifiedBy"", p.""Discount"",
                c.""Id"", c.""Name"",
                sc.""Id"", sc.""Name"",
                s.""Id"", s.""Name""
            FROM products p
            JOIN categories c ON p.""CategoryId"" = c.""Id""
            JOIN subcategories sc ON p.""SubcategoryId"" = sc.""Id""
            LEFT JOIN suppliers s ON p.""SupplierId"" = s.""Id""
            WHERE 
                (p.""CategoryId"" = ANY(@CategoryIds::int[]) OR cardinality(@CategoryIds) = 0)
                AND (p.""SubcategoryId"" = ANY(@SubcategoryIds::int[]) OR cardinality(@SubcategoryIds) = 0)
                AND (@SupplierId IS NULL OR p.""SupplierId"" = @SupplierId)
                AND (@ProductName IS NULL OR p.""Name"" ILIKE '%' || @ProductName || '%')";


        using var connection = db.CreateConnection();

        var productDict = new Dictionary<int, ProductDto>();

        var products = (await connection.QueryAsync<ProductDto, Category, Subcategory, Supplier, ProductDto>(
            sql,
            (product, category, subcategory, supplier) =>
            {
                product.Category = category;
                product.Subcategory = subcategory;
                product.Supplier = supplier;
                productDict[product.Id] = product;
                return product;
            },
            new { 
                CategoryIds = categoryIds ?? Array.Empty<int>(), 
                SubcategoryIds = subcategoryIds ?? Array.Empty<int>(), 
                SupplierId = supplierId, 
                ProductName = productName 
            },
            splitOn: "Id,Id,Id"
        )).Distinct().ToList();

        // Second query: fetch images for all products
        var imageSql = @"SELECT ""ProductId"", ""ImageUrl"" FROM product_images WHERE ""ProductId"" = ANY(@Ids)";
        var productIds = productDict.Keys.ToArray();

        var images = await connection.QueryAsync<(int ProductId, string ImageUrl)>(imageSql, new { Ids = productIds });

        foreach (var img in images)
        {
            if (productDict.TryGetValue(img.ProductId, out var prod))
            {
                prod.ImageUrls ??= Array.Empty<string>();
                prod.ImageUrls = prod.ImageUrls.Append(img.ImageUrl).ToArray();
            }
        }

        return Results.Ok(products);
});



        app.MapPost("/", async (ApplicationDbContext db, ProductPost productdto) =>
        {
            const string insertProductSql = @"
                INSERT INTO products 
                (""Name"", ""Description"", ""Price"", ""Amount"", ""CreatedBy"", ""LastModified"", ""LastModifiedBy"", ""CategoryId"", ""SubcategoryId"", ""SupplierId"")
                VALUES 
                (@Name, @Description, @Price, @Amount, @CreatedBy, @LastModified, @LastModifiedBy, @CategoryId, @SubcategoryId, @SupplierId)
                RETURNING ""Id"";
                ";

            using var connection = db.CreateConnection();
            
            var newProductId = await connection.ExecuteScalarAsync<int>(insertProductSql, productdto);

            // 2. Insert image URLs if provided
            if (productdto.ImageUrls is not null && productdto.ImageUrls.Any())
            {
                const string insertImageSql = @"
                INSERT INTO product_images (""ProductId"", ""ImageUrl"") 
                VALUES (@ProductId, @ImageUrl);
                ";

                var imageInsertData = productdto.ImageUrls.Select(url => new 
                { 
                    ProductId = newProductId, 
                    ImageUrl = url 
                });

                await connection.ExecuteAsync(insertImageSql, imageInsertData);
            }

            return Results.Ok(new { Id = newProductId });
        });


        app.MapGet("/{id:int}", async (ApplicationDbContext db, int id) =>
        {
            const string sql = @"
                SELECT 
                    p.""Id"", p.""Name"", p.""Description"", p.""Price"", p.""Amount"", p.""Sold"",
                    p.""IsDeleted"", p.""CategoryId"", p.""SubcategoryId"", p.""SupplierId"",
                    p.""Created"", p.""CreatedBy"", p.""LastModified"", p.""LastModifiedBy"", p.""Discount"",
                    c.""Id"", c.""Name"",
                    sc.""Id"", sc.""Name"",
                    s.""Id"", s.""Name""
                FROM products p
                JOIN categories c ON p.""CategoryId"" = c.""Id""
                JOIN subcategories sc ON p.""SubcategoryId"" = sc.""Id""
                LEFT JOIN suppliers s ON p.""SupplierId"" = s.""Id""
                WHERE p.""Id"" = @Id";

            using var connection = db.CreateConnection();

            var result = await connection.QueryAsync<ProductDto, Category, Subcategory, Supplier, ProductDto>(
                sql,
                (p, c, sc, s) =>
                {
                    p.Category = c;
                    p.Subcategory = sc;
                    p.Supplier = s ?? null;
                    return p;
                },
                new { Id = id },
                splitOn: "Id,Id,Id"
            );

            var product = result.FirstOrDefault();

            if (product is null)
                return Results.NotFound();

            // Uzimamo slike
            const string imageSql = @"SELECT ""ImageUrl"" FROM product_images WHERE ""ProductId"" = @ProductId";

            var images = await connection.QueryAsync<string>(imageSql, new { ProductId = id });

            product.ImageUrls = images.ToArray();

            return Results.Ok(product);
        });




        app.MapPut("/{id:int}", async (ApplicationDbContext db, int id, ProductPost productdto) =>
        {
            const string sql = @"
        UPDATE products 
        SET 
            ""Name"" = @Name,
            ""Description"" = @Description,
            ""Price"" = @Price,
            ""Amount"" = @Amount,
            ""LastModified"" = now(),
            ""LastModifiedBy"" = @LastModifiedBy,
            ""CategoryId"" = @CategoryId,
            ""SubcategoryId"" = @SubcategoryId,
            ""SupplierId"" = @SupplierId
        WHERE ""Id"" = @Id";

            using var connection = db.CreateConnection();
            using var transaction = connection.BeginTransaction();

            var result = await connection.ExecuteAsync(sql, new
            {
                productdto.Name,
                productdto.Description,
                productdto.Price,
                productdto.Amount,
                productdto.LastModifiedBy,
                productdto.CategoryId,
                productdto.SubcategoryId,
                productdto.SupplierId,
                Id = id
            }, transaction);

            if (result == 0)
            {
                transaction.Rollback();
                return Results.NotFound();
            }

            // Obrisi postojeće slike
            const string deleteImagesSql = @"DELETE FROM product_images WHERE ""ProductId"" = @ProductId";
            await connection.ExecuteAsync(deleteImagesSql, new { ProductId = id }, transaction);

            // Ubaci nove slike ako postoje
            if (productdto.ImageUrls is not null && productdto.ImageUrls.Any())
            {
                const string insertImagesSql = @"
            INSERT INTO product_images (""ProductId"", ""ImageUrl"") 
            VALUES (@ProductId, @ImageUrl)";

                var imageInsertData = productdto.ImageUrls.Select(url => new
                {
                    ProductId = id,
                    ImageUrl = url
                });

                await connection.ExecuteAsync(insertImagesSql, imageInsertData, transaction);
            }

            transaction.Commit();
            return Results.Ok(result);
        });


        app.MapDelete("/{id:int}", async (ApplicationDbContext db, int id) =>
        {
            const string sql = "DELETE FROM products WHERE \"Id\" = @Id";
            using var connection = db.CreateConnection();

            var result = await connection.ExecuteAsync(sql, new { Id = id });

            return result == 0 ? Results.NotFound() : Results.Ok(result);
        });

        app.MapPost("/upload", async (IFormFile file) =>
        {
            if (file == null || file.Length == 0)
            {
                return Results.BadRequest("Invalid file.");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            //Log.Information("FileExtension is {FileExtension}, filename is {FileName}", fileExtension, file.FileName);

            if (!allowedExtensions.Contains(fileExtension))
            {
                return Results.BadRequest("File type not supported.");
            }

            var allowedMimeTypes = new[] { "image/jpeg", "image/png" };
            //Log.Information("Tip fajla je {TipFajla}", file.ContentType);
            if (!allowedMimeTypes.Contains(file.ContentType))
            {
                return Results.BadRequest("Invalid file type.");
            }

            // Definišite putanju za direktorijum i fajl
            var uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            var fileName = Guid.NewGuid() + fileExtension;
            var filePath = Path.Combine(uploadDirectory, fileName);

            // Proverite i kreirajte direktorijum ako ne postoji
            if (!Directory.Exists(uploadDirectory))
            {
                Directory.CreateDirectory(uploadDirectory);
            }

            // Snimanje fajla
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            
            return Results.Ok(fileName);
        }).DisableAntiforgery();
        
        /*app.MapPost("/image_link/{id:int}", async (ApplicationDbContext db,ImageLinkPost imageLink, int id) =>
        {
            const string sql = @"INSERT INTO product_images 
                (""ImageUrl"", ""ProductId"")
                VALUES (@ImageUrl, @ProductId)";
            
            using var connection = db.CreateConnection();

            var result = await connection.ExecuteAsync(sql, new
            {
                imageLink.ImageUrl,
                imageLink.ProductId
            });

            return result == 0 ? Results.NotFound() : Results.Ok(result);
        });*/

    }
}

