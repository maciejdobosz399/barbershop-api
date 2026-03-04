using Asp.Versioning;
using Barbershop.Application.DTOs.Appointments;
using Barbershop.Application.Features.Appointments.Commands;
using Barbershop.Application.Features.Appointments.Queries;
using Barbershop.Domain.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Barbershop.WebAPI.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/appointments")]
public class AppointmentsController : ApiControllerBase
{
    private readonly IMessageBus _messageBus;
    private readonly ILogger<AppointmentsController> _logger;

    public AppointmentsController(IMessageBus messageBus, ILogger<AppointmentsController> logger)
    {
        _messageBus = messageBus;
        _logger = logger;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAppointmentsAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("Retrieving appointments, Page={Page}, PageSize={PageSize}", page, pageSize);
        var query = new GetAppointmentsQuery(page, pageSize);
        var result = await _messageBus.InvokeAsync<Result<IReadOnlyList<AppointmentResponse>>>(query);

        return ToActionResult(result);
    }

    [HttpGet("{id:guid}", Name = nameof(GetAppointmentByIdAsync))]
    [Authorize]
    public async Task<IActionResult> GetAppointmentByIdAsync(Guid id)
    {
        _logger.LogInformation("Retrieving appointment {AppointmentId}", id);
        var query = new GetAppointmentByIdQuery(id);
        var result = await _messageBus.InvokeAsync<Result<AppointmentResponse>>(query);

        return ToActionResult(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateAppointmentAsync([FromBody] CreateAppointmentRequest request)
    {
        var callerUserId = CurrentUserId;
        if (callerUserId is null)
        {
            _logger.LogWarning("Unauthenticated user attempted to create an appointment");
            return Unauthorized(new { Message = "User not authenticated." });
        }

        _logger.LogInformation(
            "Creating appointment for BarberId={BarberId}, ClientId={ClientId}, Type={Type}, StartAtUtc={StartAtUtc}",
            request.BarberId, request.ClientId, request.Type, request.StartAtUtc);

        var command = new CreateAppointmentCommand(
            request.StartAtUtc,
            request.Type,
            request.BarberId,
            request.ClientId,
            callerUserId.Value,
            IsAdmin);

        var result = await _messageBus.InvokeAsync<Result<AppointmentResponse>>(command);

        return ToActionResult(result, () =>
        {
            _logger.LogInformation("Appointment {AppointmentId} created successfully", result.Value.Id);
            return CreatedAtRoute(
                nameof(GetAppointmentByIdAsync),
                new { id = result.Value.Id },
                result.Value);
        });
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> UpdateAppointmentAsync(
        Guid id,
        [FromBody] UpdateAppointmentRequest request)
    {
        if (CurrentUserId is not { } callerUserId)
        {
            _logger.LogWarning("Unauthenticated user attempted to update appointment {AppointmentId}", id);
            return Unauthorized(new { Message = "User not authenticated." });
        }

        _logger.LogInformation("Updating appointment {AppointmentId}", id);

        var command = new UpdateAppointmentCommand(
            id,
            request.StartAtUtc,
            request.Type,
            request.BarberId,
            callerUserId,
            IsAdmin);

        var result = await _messageBus.InvokeAsync<Result<AppointmentResponse>>(command);

        return ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> CancelAppointmentAsync(Guid id)
    {
        if (CurrentUserId is not { } callerUserId)
        {
            _logger.LogWarning("Unauthenticated user attempted to cancel appointment {AppointmentId}", id);
            return Unauthorized(new { Message = "User not authenticated." });
        }

        _logger.LogInformation("Cancelling appointment {AppointmentId} by UserId={UserId}", id, callerUserId);

        var command = new CancelAppointmentCommand(id, callerUserId, IsAdmin);
        var result = await _messageBus.InvokeAsync<Result<bool>>(command);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Appointment {AppointmentId} cancelled successfully", id);
            return NoContent();
        }

        return ToActionResult(result);
    }
}
