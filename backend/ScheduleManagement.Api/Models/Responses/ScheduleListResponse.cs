namespace ScheduleManagement.Api.Models.Responses;

public sealed class ScheduleListResponse
{
    public int ScheduleId { get; init; }

    public string FlightNumber { get; init; } = string.Empty;

    public string Origin { get; init; } = string.Empty;

    public string Destination { get; init; } = string.Empty;

    public TimeSpan DepartureTime { get; init; }

    public TimeSpan ArrivalTime { get; init; }

    public bool ArrivesNextDay { get; init; }

    public string AircraftType { get; init; } = string.Empty;

    public string DaysOfOperation { get; init; } = string.Empty;

    public DateTime EffectiveFrom { get; init; }

    public DateTime? EffectiveTo { get; init; }

    public string Status { get; init; } = string.Empty;
}