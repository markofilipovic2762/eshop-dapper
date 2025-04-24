using System.Text;
using EshopDapper.Data;
using EshopDapper.Endpoints;
using EshopDapper.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value!)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddScoped<AuthService>();

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

//app.UseHttpsRedirection();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();


var categoryGroup = app.MapGroup("/categories").WithTags("Categories");
var productGroup = app.MapGroup("/products").WithTags("Products");
var subcategoryGroup = app.MapGroup("/subcategories").WithTags("Subcategories");
var orderGroup = app.MapGroup("/orders").WithTags("Orders");
var supplierGroup = app.MapGroup("/suppliers").WithTags("Suppliers");
var authGroup = app.MapGroup("/auth").WithTags("Authentication");
authGroup.MapAuthEndpoints();
supplierGroup.MapSupplierEndpoints();
categoryGroup.MapCategoryEndpoints();
productGroup.MapProductEndpoints();
subcategoryGroup.MapSubcategoryEndpoints();
orderGroup.MapOrderEndpoints();
app.UseCors();
app.Run();
