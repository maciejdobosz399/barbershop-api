using Asp.Versioning;
using Barbershop.Application.DTOs.Appointments;
using Barbershop.Application.DTOs.Barbers;
using Barbershop.Application.Features.Appointments.Queries;
using Barbershop.Application.Features.Barbers.Commands;
using Barbershop.Application.Features.Barbers.Queries;
using Barbershop.Domain.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Barbershop.WebAPI.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/barbers")]
public class BarbersController : ApiControllerBase
{
	private readonly IMessageBus _messageBus;
	private readonly ILogger<BarbersController> _logger;

	public BarbersController(IMessageBus messageBus, ILogger<BarbersController> logger)
	{
		_messageBus = messageBus;
		_logger = logger;
	}

	[HttpGet]
	public async Task<IActionResult> GetBarbersAsync(
		[FromQuery] int page = 1,
		[FromQuery] int pageSize = 10)
	{
		_logger.LogInformation("Retrieving barbers, Page={Page}, PageSize={PageSize}", page, pageSize);
		var query = new GetBarbersQuery(page, pageSize);
		var result = await _messageBus.InvokeAsync<Result<IReadOnlyList<BarberResponse>>>(query);

		return ToActionResult(result);
	}

	[HttpGet("{id:guid}", Name = nameof(GetBarberByIdAsync))]
	public async Task<IActionResult> GetBarberByIdAsync(
		Guid id)
	{
		_logger.LogInformation("Retrieving barber {BarberId}", id);
		var query = new GetBarberByIdQuery(id);
		var result = await _messageBus.InvokeAsync<Result<BarberResponse>>(query);

		return ToActionResult(result);
	}

	[HttpPost]
	[Authorize]
	public async Task<IActionResult> CreateBarberAsync([FromBody] CreateBarberRequest request)
	{
		_logger.LogInformation("Creating barber {FirstName} {LastName}", request.FirstName, request.LastName);

		var command = new CreateBarberCommand(
			request.FirstName,
			request.LastName,
			request.DateOfBirth,
			request.BarberLevel,
			request.PhoneNumber,
			request.Description);

		var result = await _messageBus.InvokeAsync<Result<BarberResponse>>(command);

		return ToActionResult(result, () =>
		{
			_logger.LogInformation("Barber {BarberId} created successfully", result.Value.Id);
			return CreatedAtRoute(
				nameof(GetBarberByIdAsync),
				new { id = result.Value.Id },
				result.Value);
		});
	}

	[HttpPut("{id:guid}")]
	[Authorize]
	public async Task<IActionResult> UpdateBarberAsync(
		Guid id,
		[FromBody] UpdateBarberRequest request)
	{
		_logger.LogInformation("Updating barber {BarberId}", id);

		var command = new UpdateBarberCommand(
			id,
			request.FirstName,
			request.LastName,
			request.DateOfBirth,
			request.BarberLevel,
			request.PhoneNumber,
			request.Description);

		var result = await _messageBus.InvokeAsync<Result<BarberResponse>>(command);

		return ToActionResult(result);
	}

	[HttpDelete("{id:guid}")]
	[Authorize]
	public async Task<IActionResult> DeleteBarberAsync(Guid id)
	{
		_logger.LogInformation("Deleting barber {BarberId}", id);

		var command = new DeleteBarberCommand(id);
		var result = await _messageBus.InvokeAsync<Result<bool>>(command);

		if (result.IsSuccess)
		{
			_logger.LogInformation("Barber {BarberId} deleted successfully", id);
			return NoContent();
		}

		return ToActionResult(result);
	}

	[HttpGet("{id:guid}/appointments")]
	public async Task<IActionResult> GetBarberAppointmentsAsync(
		Guid id,
		[FromQuery] int page = 1,
		[FromQuery] int pageSize = 10)
	{
		_logger.LogInformation("Retrieving appointments for barber {BarberId}, Page={Page}, PageSize={PageSize}", id, page, pageSize);
		var query = new GetAppointmentsByBarberIdQuery(id, page, pageSize);
		var result = await _messageBus.InvokeAsync<Result<IReadOnlyList<AppointmentResponse>>>(query);

		return ToActionResult(result);
	}
}