using Microsoft.AspNetCore.Mvc;
using ScheduleManagement.Api.Common;
using ScheduleManagement.Api.Models.Requests;
using ScheduleManagement.Api.Models.Responses;
using ScheduleManagement.Api.Services;

namespace ScheduleManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class SchedulesController : ControllerBase
{
    private readonly IScheduleService _scheduleService;

    public SchedulesController(
        IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }


    //Get all schedules
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

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var schedules =
                await _scheduleService.GetAllAsync(
                    originAirportId,
                    destinationAirportId,
                    status,
                    cancellationToken);

            var response =
                schedules
                    .Select(ToListResponse)
                    .ToList();

            return Ok(response);
        }
        catch (ScheduleValidationException exception)
        {
            AddValidationErrors(exception);

            return ValidationProblem(ModelState);
        }
    }


    //Get schedule by ID

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

        return Ok(ToDetailsResponse(schedule));
    }


   //Create new schedule

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
                ToDetailsResponse(schedule);

            return CreatedAtAction(
                nameof(GetScheduleById),
                new { id = scheduleId },
                response);
        }
        catch (ScheduleValidationException exception)
        {
            AddValidationErrors(exception);

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


   //Update full schedule

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

            return Ok(ToDetailsResponse(schedule));
        }
        catch (ScheduleValidationException exception)
        {
            AddValidationErrors(exception);

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


   //Update schedule status only

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

            return Ok(ToDetailsResponse(schedule));
        }
        catch (ScheduleValidationException exception)
        {
            AddValidationErrors(exception);

            return ValidationProblem(ModelState);
        }
    }


    //Delete schedule by ID

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


    //Private helper methods
    private static ScheduleListResponse ToListResponse(
        Models.Entities.ScheduleListRow schedule)
    {
        return new ScheduleListResponse
        {
            ScheduleId =
                schedule.ScheduleId,

            FlightNumber =
                schedule.FlightNumber,

            Origin =
                $"{schedule.OriginIataCode} - {schedule.OriginCity}",

            Destination =
                $"{schedule.DestinationIataCode} - {schedule.DestinationCity}",

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
                schedule.Status
        };
    }


    private static ScheduleDetailsResponse ToDetailsResponse(
        Models.Entities.FlightSchedule schedule)
    {
        return new ScheduleDetailsResponse
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
    }


    //Private validation helper method
    private void AddValidationErrors(
        ScheduleValidationException exception)
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
    }
}