namespace ScheduleManagement.Api.Models.Responses;

public sealed class AirportResponse
{
    public int AirportId { get; init; }

    public string IataCode { get; init; } = string.Empty;

    public string AirportName { get; init; } = string.Empty;

    public string City { get; init; } = string.Empty;

    public string CountryCode { get; init; } = string.Empty;
}