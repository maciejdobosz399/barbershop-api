namespace Barbershop.Infrastructure.Identity;

public interface IJwtTokenService
{
    string GenerateAccessToken(ApplicationUser user, IList<string> roles);
    RefreshToken GenerateRefreshToken(Guid userId);
    Guid? ValidateAccessToken(string token);
}
