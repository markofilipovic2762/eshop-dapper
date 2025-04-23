namespace EshopDapper.DTO.User;

public record UserRegisterDto(
    string Name,
    string Username,
    string Email,
    string Password
    );
    
    
public record UserLoginDto(
    string Username,
    string Email,
    string Password
    );

public record UserDto(
    int Id,
    string Username, 
    string Email, 
    string Role);