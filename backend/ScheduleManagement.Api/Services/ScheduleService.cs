using ScheduleManagement.Api.Models.Entities;
using ScheduleManagement.Api.Repositories;
using ScheduleManagement.Api.Common;
using ScheduleManagement.Api.Models.Requests;

namespace ScheduleManagement.Api.Services;

public sealed class ScheduleService : IScheduleService
{
    private readonly IScheduleRepository _scheduleRepository;

    public ScheduleService(
        IScheduleRepository scheduleRepository)
    {
        _scheduleRepository = scheduleRepository;
    }

    public async Task<IReadOnlyList<ScheduleListRow>> GetAllAsync(
        int? originAirportId,
        int? destinationAirportId,
        string? status,
        CancellationToken cancellationToken = default)
    {
        return await _scheduleRepository.GetAllAsync(
            originAirportId,
            destinationAirportId,
            status,
            cancellationToken);
    }

    public async Task<int> CreateAsync(
    CreateScheduleRequest request,
    CancellationToken cancellationToken = default)
{
    var errors =
        new Dictionary<string, List<string>>();

    void AddError(string field, string message)
    {
        if (!errors.TryGetValue(field, out var messages))
        {
            messages = new List<string>();
            errors[field] = messages;
        }

        messages.Add(message);
    }

    var flightNumber =
        request.FlightNumber.Trim().ToUpperInvariant();

    if (!System.Text.RegularExpressions.Regex.IsMatch(
            flightNumber,
            @"^[A-Z]{2}\d{3,4}$"))
    {
        AddError(
            "flightNumber",
            "Flight number must contain a two-letter carrier code followed by 3 or 4 digits.");
    }

    if (request.OriginAirportId <= 0)
    {
        AddError(
            "originAirportId",
            "Origin airport is required.");
    }

    if (request.DestinationAirportId <= 0)
    {
        AddError(
            "destinationAirportId",
            "Destination airport is required.");
    }

    if (request.OriginAirportId > 0 &&
        request.DestinationAirportId > 0 &&
        request.OriginAirportId ==
        request.DestinationAirportId)
    {
        AddError(
            "destinationAirportId",
            "Origin and destination airports must be different.");
    }

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

    var allowedAircraftTypes =
        new[] { "A320", "A330", "A350" };

    var aircraftType =
        request.AircraftType.Trim().ToUpperInvariant();

    if (!allowedAircraftTypes.Contains(aircraftType))
    {
        AddError(
            "aircraftType",
            "Aircraft type must be A320, A330, or A350.");
    }

    var daysOfOperation =
        request.DaysOfOperation.Trim();

    var validDaysFormat =
        System.Text.RegularExpressions.Regex.IsMatch(
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

    var allowedStatuses =
        new[] { "Draft", "Published", "Suspended" };

    var matchedStatus =
        allowedStatuses.FirstOrDefault(
            value => value.Equals(
                request.Status.Trim(),
                StringComparison.OrdinalIgnoreCase));

    if (matchedStatus is null)
    {
        AddError(
            "status",
            "Status must be Draft, Published, or Suspended.");
    }

    if (errors.Count > 0)
    {
        throw new ScheduleValidationException(
            errors.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.ToArray()));
    }

    var schedule = new FlightSchedule
    {
        FlightNumber = flightNumber,

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

        ModifiedOn = null
    };

    return await _scheduleRepository.CreateAsync(
        schedule,
        cancellationToken);
}
}