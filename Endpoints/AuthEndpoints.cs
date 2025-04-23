using Dapper;
using EshopDapper.Data;
using EshopDapper.DTO;
using EshopDapper.DTO.User;
using EshopDapper.Entities;
using EshopDapper.Services;

namespace EshopDapper.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/register", async (ApplicationDbContext db, AuthService authService, UserRegisterDto request) =>
        {
            const string checkUserSql = "SELECT COUNT(*) FROM users WHERE \"Username\" = @Username OR \"Email\" = @Email";
            using var connection = db.CreateConnection();
            
            var exists = await connection.ExecuteScalarAsync<int>(checkUserSql, 
                new { request.Username, request.Email });
            
            if (exists > 0)
                return Results.BadRequest("User already exists");

            authService.CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            const string sql = @"INSERT INTO users 
                (""Name"", ""Username"", ""Email"", ""PasswordHash"", ""PasswordSalt"") 
                VALUES (@Name,@Username, @Email, @PasswordHash, @PasswordSalt)";

            var user = new User
            {
                Name = request.Name,
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            await connection.ExecuteAsync(sql, user);

            return Results.Ok("User successfully registered");
        });

        app.MapPost("/login", async (ApplicationDbContext db, AuthService authService, UserLoginDto request) =>
        {
            const string sql = "SELECT * FROM users WHERE \"Username\" = @Username";
            using var connection = db.CreateConnection();
            
            var user = await connection.QuerySingleOrDefaultAsync<User>(sql, new { request.Username });
            
            if (user == null)
                return Results.NotFound("User not found");

            if (!authService.VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
                return Results.BadRequest("Wrong password");

            var token = authService.CreateToken(user);

            return Results.Ok(token);
        });
    }
}