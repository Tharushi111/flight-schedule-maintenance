namespace ScheduleManagement.Api.Models.Responses;

public sealed class ScheduleDetailsResponse
{
    public int ScheduleId { get; init; }

    public string FlightNumber { get; init; } = string.Empty;

    public int OriginAirportId { get; init; }

    public int DestinationAirportId { get; init; }

    public TimeSpan DepartureTime { get; init; }

    public TimeSpan ArrivalTime { get; init; }

    public bool ArrivesNextDay { get; init; }

    public string AircraftType { get; init; } = string.Empty;

    public string DaysOfOperation { get; init; } = string.Empty;

    public DateTime EffectiveFrom { get; init; }

    public DateTime? EffectiveTo { get; init; }

    public string Status { get; init; } = string.Empty;

    public DateTime CreatedOn { get; init; }

    public DateTime? ModifiedOn { get; init; }
}