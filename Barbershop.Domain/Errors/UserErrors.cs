using Barbershop.Domain.Abstractions;

namespace Barbershop.Domain.Errors;

public static class UserErrors
{
    public static Error InvalidCredentials => new("User.InvalidCredentials", "Invalid email or password.");
    public static Error AccountLockedOut => new("User.AccountLockedOut", "Account is locked out. Please try again later.");
    public static Error NotFound => new("User.NotFound", "User not found.");
    public static Error InvalidRefreshToken => new("User.InvalidRefreshToken", "Invalid or expired refresh token.");
    public static Error InvalidAccessToken => new("User.InvalidAccessToken", "Invalid access token.");
    public static Error NotAuthenticated => new("User.NotAuthenticated", "User not authenticated.");
    
    public static Error ValidationFailed(IEnumerable<string> errors) => 
        new("User.ValidationFailed", string.Join("; ", errors));
}
