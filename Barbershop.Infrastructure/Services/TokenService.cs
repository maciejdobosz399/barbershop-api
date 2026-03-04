using Barbershop.Application.Abstractions;
using Barbershop.Application.DTOs.Users;
using Barbershop.Domain.Interfaces;
using Barbershop.Infrastructure.DbContexts;
using Barbershop.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Barbershop.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ApplicationDbContext _dbContext;
    private readonly JwtSettings _jwtSettings;
    private readonly IIdentityService _identityService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public TokenService(
        IJwtTokenService jwtTokenService,
        ApplicationDbContext dbContext,
        IOptions<JwtSettings> jwtSettings,
        IIdentityService identityService,
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager)
    {
        _jwtTokenService = jwtTokenService;
        _dbContext = dbContext;
        _jwtSettings = jwtSettings.Value;
        _identityService = identityService;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<AuthResponse> GenerateAuthResponseAsync(Guid userId)
    {
        var user = await _dbContext.Users.FindAsync(userId)
            ?? throw new InvalidOperationException($"User with ID {userId} not found.");

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtTokenService.GenerateAccessToken(user, roles);
        var refreshToken = _jwtTokenService.GenerateRefreshToken(userId);

        _dbContext.RefreshTokens.Add(refreshToken);
        await _unitOfWork.SaveChangesAsync();

        var userResponse = await _identityService.GetUserByIdAsync(userId)
            ?? throw new InvalidOperationException($"User with ID {userId} not found.");

        return new AuthResponse(
            accessToken,
            refreshToken.Token,
            DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            userResponse);
    }

    public Guid? ValidateAccessToken(string token)
    {
        return _jwtTokenService.ValidateAccessToken(token);
    }

    public async Task<(bool IsValid, Guid? UserId)> ValidateRefreshTokenAsync(string accessToken, string refreshToken)
    {
        var userId = _jwtTokenService.ValidateAccessToken(accessToken);

        if (userId is null)
        {
            return (false, null);
        }

        var storedRefreshToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.UserId == userId);

        if (storedRefreshToken is null || !storedRefreshToken.IsActive)
        {
            return (false, null);
        }

        return (true, userId);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, string? replacedByToken = null)
    {
        var storedRefreshToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (storedRefreshToken is not null && storedRefreshToken.IsActive)
        {
            storedRefreshToken.RevokedAt = DateTime.UtcNow;
            storedRefreshToken.ReplacedByToken = replacedByToken;
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
