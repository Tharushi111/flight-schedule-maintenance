namespace ScheduleManagement.Api.Models.Entities;

public sealed class Airport
{
    public int AirportId { get; init; }

    public string IataCode { get; init; } = string.Empty;

    public string AirportName { get; init; } = string.Empty;

    public string City { get; init; } = string.Empty;

    public string CountryCode { get; init; } = string.Empty;

    public bool IsActive { get; init; }
}