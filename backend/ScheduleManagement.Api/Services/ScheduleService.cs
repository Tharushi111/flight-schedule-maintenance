using System.Text.RegularExpressions;
using ScheduleManagement.Api.Common;
using ScheduleManagement.Api.Models.Entities;
using ScheduleManagement.Api.Models.Requests;
using ScheduleManagement.Api.Repositories;

namespace ScheduleManagement.Api.Services;

public sealed class ScheduleService : IScheduleService
{
    private readonly IScheduleRepository _scheduleRepository;
    private readonly IAirportRepository _airportRepository;

    public ScheduleService(
        IScheduleRepository scheduleRepository,
        IAirportRepository airportRepository)
    {
        _scheduleRepository = scheduleRepository;
        _airportRepository = airportRepository;
    }


    // Get all schedules
    public async Task<IReadOnlyList<ScheduleListRow>> GetAllAsync(
        int? originAirportId,
        int? destinationAirportId,
        string? status,
        CancellationToken cancellationToken = default)
    {
        string? normalizedStatus = null;

        if (!string.IsNullOrWhiteSpace(status))
        {
            normalizedStatus =
                ScheduleConstants.AllowedStatuses.FirstOrDefault(
                    value => value.Equals(
                        status.Trim(),
                        StringComparison.OrdinalIgnoreCase));

            if (normalizedStatus is null)
            {
                throw new ScheduleValidationException(
                    new Dictionary<string, string[]>
                    {
                        ["status"] =
                        [
                            "Status must be Draft, Published, or Suspended."
                        ]
                    });
            }
        }

        return await _scheduleRepository.GetAllAsync(
            originAirportId,
            destinationAirportId,
            normalizedStatus,
            cancellationToken);
    }


    // Get schedule by ID
        public async Task<FlightSchedule?> GetByIdAsync(
        int scheduleId,
        CancellationToken cancellationToken = default)
    {
        return await _scheduleRepository.GetByIdAsync(
            scheduleId,
            cancellationToken);
    }


    // Create new schedule
    public async Task<int> CreateAsync(
        CreateScheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors =
            new Dictionary<string, List<string>>();

        void AddError(
            string field,
            string message)
        {
            if (!errors.TryGetValue(
                    field,
                    out var messages))
            {
                messages = new List<string>();
                errors[field] = messages;
            }

            messages.Add(message);
        }


        // Flight Number Validation

        var flightNumber =
            request.FlightNumber
                .Trim()
                .ToUpperInvariant();

        if (!Regex.IsMatch(
                flightNumber,
                ScheduleConstants.FlightNumberPattern))
        {
            AddError(
                "flightNumber",
                "Flight number must contain a two-letter carrier code followed by 3 or 4 digits.");
        }


        // Origin Airport Validation

        if (request.OriginAirportId <= 0)
        {
            AddError(
                "originAirportId",
                "Origin airport is required.");
        }
        else
        {
            var originExists =
                await _airportRepository.ExistsAsync(
                    request.OriginAirportId,
                    cancellationToken);

            if (!originExists)
            {
                AddError(
                    "originAirportId",
                    "Origin airport does not exist.");
            }
        }


        // Destination Airport Validation

        if (request.DestinationAirportId <= 0)
        {
            AddError(
                "destinationAirportId",
                "Destination airport is required.");
        }
        else
        {
            var destinationExists =
                await _airportRepository.ExistsAsync(
                    request.DestinationAirportId,
                    cancellationToken);

            if (!destinationExists)
            {
                AddError(
                    "destinationAirportId",
                    "Destination airport does not exist.");
            }
        }


        // Origin must differ from Destination

        if (request.OriginAirportId > 0 &&
            request.DestinationAirportId > 0 &&
            request.OriginAirportId ==
            request.DestinationAirportId)
        {
            AddError(
                "destinationAirportId",
                "Origin and destination airports must be different.");
        }


        // Time Validation

        if (!request.DepartureTime.HasValue)
        {
            AddError(
                "departureTime",
                "Departure time is required.");
        }

        if (!request.ArrivalTime.HasValue)
        {
            AddError(
                "arrivalTime",
                "Arrival time is required.");
        }

       
        // Aircraft Type Validation
        
        var aircraftType =
            request.AircraftType
                .Trim()
                .ToUpperInvariant();

        if (!ScheduleConstants
                .AllowedAircraftTypes
                .Contains(aircraftType))
        {
            AddError(
                "aircraftType",
                "Aircraft type must be A320, A330, or A350.");
        }


        // Days of Operation Validation

        var daysOfOperation =
            request.DaysOfOperation.Trim();

        var validDaysFormat =
            Regex.IsMatch(
                daysOfOperation,
                @"^[1.][2.][3.][4.][5.][6.][7.]$");

        if (!validDaysFormat)
        {
            AddError(
                "daysOfOperation",
                "Days of operation must contain seven positions for Monday through Sunday.");
        }
        else if (!daysOfOperation.Any(char.IsDigit))
        {
            AddError(
                "daysOfOperation",
                "At least one operating day must be selected.");
        }


        // Effective Date Validation

        if (!request.EffectiveFrom.HasValue)
        {
            AddError(
                "effectiveFrom",
                "Effective from date is required.");
        }

        if (request.EffectiveFrom.HasValue &&
            request.EffectiveTo.HasValue &&
            request.EffectiveTo.Value.Date <
            request.EffectiveFrom.Value.Date)
        {
            AddError(
                "effectiveTo",
                "Effective to date cannot be earlier than effective from date.");
        }


        // Status Validation

        var matchedStatus =
            ScheduleConstants.AllowedStatuses
                .FirstOrDefault(
                    value => value.Equals(
                        request.Status.Trim(),
                        StringComparison.OrdinalIgnoreCase));

        if (matchedStatus is null)
        {
            AddError(
                "status",
                "Status must be Draft, Published, or Suspended.");
        }


        
        // Throw Validation Errors

        if (errors.Count > 0)
        {
            throw new ScheduleValidationException(
                errors.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.ToArray()));
        }


        
        // Create Internal Entity

        var schedule =
            new FlightSchedule
            {
                FlightNumber =
                    flightNumber,

                OriginAirportId =
                    request.OriginAirportId,

                DestinationAirportId =
                    request.DestinationAirportId,

                DepartureTime =
                    request.DepartureTime!.Value,

                ArrivalTime =
                    request.ArrivalTime!.Value,

                AircraftType =
                    aircraftType,

                DaysOfOperation =
                    daysOfOperation,

                EffectiveFrom =
                    request.EffectiveFrom!.Value.Date,

                EffectiveTo =
                    request.EffectiveTo?.Date,

                Status =
                    matchedStatus!,

                CreatedOn =
                    DateTime.UtcNow,

                ModifiedOn =
                    null
            };


        // Save using Repository

        return await _scheduleRepository.CreateAsync(
            schedule,
            cancellationToken);
    }


    // Update full schedule
    public async Task<FlightSchedule?> UpdateAsync(
        int scheduleId,
        UpdateScheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        var existingSchedule =
            await _scheduleRepository.GetByIdAsync(
                scheduleId,
                cancellationToken);

        if (existingSchedule is null)
        {
            return null;
        }


        var errors =
            new Dictionary<string, List<string>>();

        void AddError(
            string field,
            string message)
        {
            if (!errors.TryGetValue(
                    field,
                    out var messages))
            {
                messages = new List<string>();
                errors[field] = messages;
            }

            messages.Add(message);
        }


        // Flight Number Validation

        var flightNumber =
            request.FlightNumber
                .Trim()
                .ToUpperInvariant();

        if (!Regex.IsMatch(
                flightNumber,
                ScheduleConstants.FlightNumberPattern))
        {
            AddError(
                "flightNumber",
                "Flight number must contain a two-letter carrier code followed by 3 or 4 digits.");
        }


        // Origin Airport Validation

        if (request.OriginAirportId <= 0)
        {
            AddError(
                "originAirportId",
                "Origin airport is required.");
        }
        else
        {
            var originExists =
                await _airportRepository.ExistsAsync(
                    request.OriginAirportId,
                    cancellationToken);

            if (!originExists)
            {
                AddError(
                    "originAirportId",
                    "Origin airport does not exist.");
            }
        }


        // Destination Airport Validation

        if (request.DestinationAirportId <= 0)
        {
            AddError(
                "destinationAirportId",
                "Destination airport is required.");
        }
        else
        {
            var destinationExists =
                await _airportRepository.ExistsAsync(
                    request.DestinationAirportId,
                    cancellationToken);

            if (!destinationExists)
            {
                AddError(
                    "destinationAirportId",
                    "Destination airport does not exist.");
            }
        }


        // Origin must differ from Destination

        if (request.OriginAirportId > 0 &&
            request.DestinationAirportId > 0 &&
            request.OriginAirportId ==
            request.DestinationAirportId)
        {
            AddError(
                "destinationAirportId",
                "Origin and destination airports must be different.");
        }


        // Time Validation

        if (!request.DepartureTime.HasValue)
        {
            AddError(
                "departureTime",
                "Departure time is required.");
        }

        if (!request.ArrivalTime.HasValue)
        {
            AddError(
                "arrivalTime",
                "Arrival time is required.");
        }


        // Aircraft Type Validation

        var aircraftType =
            request.AircraftType
                .Trim()
                .ToUpperInvariant();

        if (!ScheduleConstants
                .AllowedAircraftTypes
                .Contains(aircraftType))
        {
            AddError(
                "aircraftType",
                "Aircraft type must be A320, A330, or A350.");
        }


        // Days of Operation Validation

        var daysOfOperation =
            request.DaysOfOperation.Trim();

        var validDaysFormat =
            Regex.IsMatch(
                daysOfOperation,
                @"^[1.][2.][3.][4.][5.][6.][7.]$");

        if (!validDaysFormat)
        {
            AddError(
                "daysOfOperation",
                "Days of operation must contain seven positions for Monday through Sunday.");
        }
        else if (!daysOfOperation.Any(char.IsDigit))
        {
            AddError(
                "daysOfOperation",
                "At least one operating day must be selected.");
        }


        // Effective Date Validation

        if (!request.EffectiveFrom.HasValue)
        {
            AddError(
                "effectiveFrom",
                "Effective from date is required.");
        }

        if (request.EffectiveFrom.HasValue &&
            request.EffectiveTo.HasValue &&
            request.EffectiveTo.Value.Date <
            request.EffectiveFrom.Value.Date)
        {
            AddError(
                "effectiveTo",
                "Effective to date cannot be earlier than effective from date.");
        }


        // Status Validation

        var matchedStatus =
            ScheduleConstants.AllowedStatuses
                .FirstOrDefault(
                    value => value.Equals(
                        request.Status.Trim(),
                        StringComparison.OrdinalIgnoreCase));

        if (matchedStatus is null)
        {
            AddError(
                "status",
                "Status must be Draft, Published, or Suspended.");
        }


        // Throw Validation Errors

        if (errors.Count > 0)
        {
            throw new ScheduleValidationException(
                errors.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.ToArray()));
        }


        // Build Updated Entity

        var updatedSchedule =
            new FlightSchedule
            {
                ScheduleId =
                    existingSchedule.ScheduleId,

                FlightNumber =
                    flightNumber,

                OriginAirportId =
                    request.OriginAirportId,

                DestinationAirportId =
                    request.DestinationAirportId,

                DepartureTime =
                    request.DepartureTime!.Value,

                ArrivalTime =
                    request.ArrivalTime!.Value,

                AircraftType =
                    aircraftType,

                DaysOfOperation =
                    daysOfOperation,

                EffectiveFrom =
                    request.EffectiveFrom!.Value.Date,

                EffectiveTo =
                    request.EffectiveTo?.Date,

                Status =
                    matchedStatus!,

                CreatedOn =
                    existingSchedule.CreatedOn,

                ModifiedOn =
                    DateTime.UtcNow
            };


        // Save Update

        var updated =
            await _scheduleRepository.UpdateAsync(
                updatedSchedule,
                cancellationToken);

        if (!updated)
        {
            return null;
        }

        return await _scheduleRepository.GetByIdAsync(
            scheduleId,
            cancellationToken);
    }


    // Update schedule status only
    public async Task<FlightSchedule?> UpdateStatusAsync(
        int scheduleId,
        UpdateScheduleStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var matchedStatus =
            ScheduleConstants.AllowedStatuses
                .FirstOrDefault(
                    value => value.Equals(
                        request.Status.Trim(),
                        StringComparison.OrdinalIgnoreCase));

        if (matchedStatus is null)
        {
            throw new ScheduleValidationException(
                new Dictionary<string, string[]>
                {
                    ["status"] =
                    [
                        "Status must be Draft, Published, or Suspended."
                    ]
                });
        }

        var updated =
            await _scheduleRepository.UpdateStatusAsync(
                scheduleId,
                matchedStatus,
                DateTime.UtcNow,
                cancellationToken);

        if (!updated)
        {
            return null;
        }

        return await _scheduleRepository.GetByIdAsync(
            scheduleId,
            cancellationToken);
    }

    // Delete schedule by ID
    public async Task<bool> DeleteAsync(
        int scheduleId,
        CancellationToken cancellationToken = default)
    {
        return await _scheduleRepository.DeleteAsync(
            scheduleId,
            cancellationToken);
    }
}