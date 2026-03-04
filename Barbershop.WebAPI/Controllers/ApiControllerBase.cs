using System.IdentityModel.Tokens.Jwt;
using Barbershop.Domain.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Barbershop.WebAPI.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected Guid? CurrentUserId
    {
        get
        {
            var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    protected bool IsAdmin => User.IsInRole("Admin");
    protected IActionResult ToActionResult<T>(Result<T> result, Func<IActionResult>? onSuccess = null)
    {
        if (result.IsSuccess)
        {
            return onSuccess?.Invoke() ?? Ok(result.Value);
        }

        var error = result.Error!;

        return error.Code.Split('.')[1] switch
        {
            "NotFound" => NotFound(new { error.Message }),
            "InvalidCredentials" or "AccountLockedOut" or "InvalidRefreshToken" 
                or "InvalidAccessToken" or "NotAuthenticated" 
                => Unauthorized(new { error.Message }),
            "Unauthorized" => Forbid(),
            _ => BadRequest(new { error.Message })
        };
    }

    protected IActionResult ToActionResult(Result result)
    {
        if (result.IsSuccess)
        {
            return Ok();
        }

        var error = result.Error!;

        return error.Code.Split('.')[1] switch
        {
            "NotFound" => NotFound(new { error.Message }),
            "InvalidCredentials" or "AccountLockedOut" or "InvalidRefreshToken"
                or "InvalidAccessToken" or "NotAuthenticated"
                => Unauthorized(new { error.Message }),
            "Unauthorized" => Forbid(),
            _ => BadRequest(new { error.Message })
        };
    }
}
