using Microsoft.AspNetCore.Mvc;
using ScheduleManagement.Api.Models.Responses;
using ScheduleManagement.Api.Services;
using ScheduleManagement.Api.Common;
using ScheduleManagement.Api.Models.Requests;

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

    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(ScheduleDetailsResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ScheduleDetailsResponse>>
        GetScheduleById(
            int id,
            CancellationToken cancellationToken)
    {
        var schedule =
            await _scheduleService.GetByIdAsync(
                id,
                cancellationToken);

        if (schedule is null)
        {
            return NotFound(new
            {
                message = "Schedule was not found."
            });
        }

        var response =
            new ScheduleDetailsResponse
            {
                ScheduleId = schedule.ScheduleId,

                FlightNumber = schedule.FlightNumber,

                OriginAirportId =
                    schedule.OriginAirportId,

                DestinationAirportId =
                    schedule.DestinationAirportId,

                DepartureTime =
                    schedule.DepartureTime,

                ArrivalTime =
                    schedule.ArrivalTime,

                ArrivesNextDay =
                    schedule.ArrivalTime <
                    schedule.DepartureTime,

                AircraftType =
                    schedule.AircraftType,

                DaysOfOperation =
                    schedule.DaysOfOperation,

                EffectiveFrom =
                    schedule.EffectiveFrom,

                EffectiveTo =
                    schedule.EffectiveTo,

                Status =
                    schedule.Status,

                CreatedOn =
                    schedule.CreatedOn,

                ModifiedOn =
                    schedule.ModifiedOn
            };

        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(ScheduleDetailsResponse),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ScheduleDetailsResponse>>
        CreateSchedule(
            [FromBody] CreateScheduleRequest request,
            CancellationToken cancellationToken)
    {
        try
        {
            var scheduleId =
                await _scheduleService.CreateAsync(
                    request,
                    cancellationToken);

            var schedule =
                await _scheduleService.GetByIdAsync(
                    scheduleId,
                    cancellationToken);

            if (schedule is null)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message =
                            "Schedule was created but could not be retrieved."
                    });
            }

            var response =
                new ScheduleDetailsResponse
                {
                    ScheduleId =
                        schedule.ScheduleId,

                    FlightNumber =
                        schedule.FlightNumber,

                    OriginAirportId =
                        schedule.OriginAirportId,

                    DestinationAirportId =
                        schedule.DestinationAirportId,

                    DepartureTime =
                        schedule.DepartureTime,

                    ArrivalTime =
                        schedule.ArrivalTime,

                    ArrivesNextDay =
                        schedule.ArrivalTime <
                        schedule.DepartureTime,

                    AircraftType =
                        schedule.AircraftType,

                    DaysOfOperation =
                        schedule.DaysOfOperation,

                    EffectiveFrom =
                        schedule.EffectiveFrom,

                    EffectiveTo =
                        schedule.EffectiveTo,

                    Status =
                        schedule.Status,

                    CreatedOn =
                        schedule.CreatedOn,

                    ModifiedOn =
                        schedule.ModifiedOn
                };

            return CreatedAtAction(
                nameof(GetScheduleById),
                new { id = scheduleId },
                response);
        }
        catch (ScheduleValidationException exception)
        {
            foreach (var error in exception.Errors)
            {
                foreach (var message in error.Value)
                {
                    ModelState.AddModelError(
                        error.Key,
                        message);
                }
            }

            return ValidationProblem(ModelState);
        }
        catch (DuplicateScheduleException exception)
        {
            return Conflict(new
            {
                message = exception.Message
            });
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(
        typeof(ScheduleDetailsResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ScheduleDetailsResponse>>
        UpdateSchedule(
            int id,
            [FromBody] UpdateScheduleRequest request,
            CancellationToken cancellationToken)
    {
        try
        {
            var schedule =
                await _scheduleService.UpdateAsync(
                    id,
                    request,
                    cancellationToken);

            if (schedule is null)
            {
                return NotFound(new
                {
                    message = "Schedule was not found."
                });
            }

            var response =
                new ScheduleDetailsResponse
                {
                    ScheduleId =
                        schedule.ScheduleId,

                    FlightNumber =
                        schedule.FlightNumber,

                    OriginAirportId =
                        schedule.OriginAirportId,

                    DestinationAirportId =
                        schedule.DestinationAirportId,

                    DepartureTime =
                        schedule.DepartureTime,

                    ArrivalTime =
                        schedule.ArrivalTime,

                    ArrivesNextDay =
                        schedule.ArrivalTime <
                        schedule.DepartureTime,

                    AircraftType =
                        schedule.AircraftType,

                    DaysOfOperation =
                        schedule.DaysOfOperation,

                    EffectiveFrom =
                        schedule.EffectiveFrom,

                    EffectiveTo =
                        schedule.EffectiveTo,

                    Status =
                        schedule.Status,

                    CreatedOn =
                        schedule.CreatedOn,

                    ModifiedOn =
                        schedule.ModifiedOn
                };

            return Ok(response);
        }
        catch (ScheduleValidationException exception)
        {
            foreach (var error in exception.Errors)
            {
                foreach (var message in error.Value)
                {
                    ModelState.AddModelError(
                        error.Key,
                        message);
                }
            }

            return ValidationProblem(ModelState);
        }
        catch (DuplicateScheduleException exception)
        {
            return Conflict(new
            {
                message = exception.Message
            });
        }
    }

    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(
        typeof(ScheduleDetailsResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ScheduleDetailsResponse>>
        UpdateScheduleStatus(
            int id,
            [FromBody] UpdateScheduleStatusRequest request,
            CancellationToken cancellationToken)
    {
        try
        {
            var schedule =
                await _scheduleService.UpdateStatusAsync(
                    id,
                    request,
                    cancellationToken);

            if (schedule is null)
            {
                return NotFound(new
                {
                    message = "Schedule was not found."
                });
            }

            var response =
                new ScheduleDetailsResponse
                {
                    ScheduleId =
                        schedule.ScheduleId,

                    FlightNumber =
                        schedule.FlightNumber,

                    OriginAirportId =
                        schedule.OriginAirportId,

                    DestinationAirportId =
                        schedule.DestinationAirportId,

                    DepartureTime =
                        schedule.DepartureTime,

                    ArrivalTime =
                        schedule.ArrivalTime,

                    ArrivesNextDay =
                        schedule.ArrivalTime <
                        schedule.DepartureTime,

                    AircraftType =
                        schedule.AircraftType,

                    DaysOfOperation =
                        schedule.DaysOfOperation,

                    EffectiveFrom =
                        schedule.EffectiveFrom,

                    EffectiveTo =
                        schedule.EffectiveTo,

                    Status =
                        schedule.Status,

                    CreatedOn =
                        schedule.CreatedOn,

                    ModifiedOn =
                        schedule.ModifiedOn
                };

            return Ok(response);
        }
        catch (ScheduleValidationException exception)
        {
            foreach (var error in exception.Errors)
            {
                foreach (var message in error.Value)
                {
                    ModelState.AddModelError(
                        error.Key,
                        message);
                }
            }

            return ValidationProblem(ModelState);
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(
        StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSchedule(
        int id,
        CancellationToken cancellationToken)
    {
        var deleted =
            await _scheduleService.DeleteAsync(
                id,
                cancellationToken);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Schedule was not found."
            });
        }

        return NoContent();
    }
}