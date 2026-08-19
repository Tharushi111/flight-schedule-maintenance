namespace ScheduleManagement.Api.Models.Entities;

public sealed class ScheduleListRow
{
    public int ScheduleId { get; init; }

    public string FlightNumber { get; init; } = string.Empty;

    public int OriginAirportId { get; init; }

    public string OriginIataCode { get; init; } = string.Empty;

    public string OriginCity { get; init; } = string.Empty;

    public int DestinationAirportId { get; init; }

    public string DestinationIataCode { get; init; } = string.Empty;

    public string DestinationCity { get; init; } = string.Empty;

    public TimeSpan DepartureTime { get; init; }

    public TimeSpan ArrivalTime { get; init; }

    public string AircraftType { get; init; } = string.Empty;

    public string DaysOfOperation { get; init; } = string.Empty;

    public DateTime EffectiveFrom { get; init; }

    public DateTime? EffectiveTo { get; init; }

    public string Status { get; init; } = string.Empty;
}