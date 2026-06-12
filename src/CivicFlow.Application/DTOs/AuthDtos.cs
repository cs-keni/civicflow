namespace CivicFlow.Application.DTOs;

public record LoginRequest(string Email, string Password);

public record UserDto(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    IList<string> Roles,
    bool IsActive);
