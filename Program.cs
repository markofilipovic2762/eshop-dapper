using EshopDapper.Data;
using EshopDapper.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<ApplicationDbContext>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var categoryGroup = app.MapGroup("/categories").WithTags("Categories");
var productGroup = app.MapGroup("/products").WithTags("Products");
var subcategoryGroup = app.MapGroup("/subcategories").WithTags("Subcategories");
var orderGroup = app.MapGroup("/orders").WithTags("Orders");
categoryGroup.MapCategoryEndpoints();
productGroup.MapProductEndpoints();
subcategoryGroup.MapSubcategoryEndpoints();
orderGroup.MapOrderEndpoints();
app.Run();
