using Microsoft.AspNetCore.Mvc;
using ScheduleManagement.Api.Models.Responses;
using ScheduleManagement.Api.Services;

namespace ScheduleManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class SchedulesController : ControllerBase
{
    private static readonly HashSet<string> AllowedStatuses =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Draft",
            "Published",
            "Suspended"
        };

    private readonly IScheduleService _scheduleService;

    public SchedulesController(
        IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<ScheduleListResponse>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<ScheduleListResponse>>>
        GetSchedules(
            [FromQuery(Name = "origin")] int? originAirportId,
            [FromQuery(Name = "destination")] int? destinationAirportId,
            [FromQuery] string? status,
            CancellationToken cancellationToken)
    {
        if (originAirportId <= 0)
        {
            ModelState.AddModelError(
                "origin",
                "Origin airport ID must be greater than zero.");
        }

        if (destinationAirportId <= 0)
        {
            ModelState.AddModelError(
                "destination",
                "Destination airport ID must be greater than zero.");
        }

        if (!string.IsNullOrWhiteSpace(status) &&
            !AllowedStatuses.Contains(status))
        {
            ModelState.AddModelError(
                "status",
                "Status must be Draft, Published, or Suspended.");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var schedules =
            await _scheduleService.GetAllAsync(
                originAirportId,
                destinationAirportId,
                status,
                cancellationToken);

        var response = schedules
            .Select(schedule => new ScheduleListResponse
            {
                ScheduleId = schedule.ScheduleId,

                FlightNumber = schedule.FlightNumber,

                Origin =
                    $"{schedule.OriginIataCode} - {schedule.OriginCity}",

                Destination =
                    $"{schedule.DestinationIataCode} - {schedule.DestinationCity}",

                DepartureTime = schedule.DepartureTime,

                ArrivalTime = schedule.ArrivalTime,

                ArrivesNextDay =
                    schedule.ArrivalTime < schedule.DepartureTime,

                AircraftType = schedule.AircraftType,

                DaysOfOperation = schedule.DaysOfOperation,

                EffectiveFrom = schedule.EffectiveFrom,

                EffectiveTo = schedule.EffectiveTo,

                Status = schedule.Status
            })
            .ToList();

        return Ok(response);
    }
}