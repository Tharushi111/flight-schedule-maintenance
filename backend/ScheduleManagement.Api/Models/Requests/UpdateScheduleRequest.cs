using System.ComponentModel.DataAnnotations;

namespace ScheduleManagement.Api.Models.Requests;

public sealed class UpdateScheduleRequest
{
    [Required]
    [RegularExpression(
        @"^[A-Za-z]{2}\d{3,4}$",
        ErrorMessage =
            "Flight number must contain a two-letter carrier code followed by 3 or 4 digits.")]
    public string FlightNumber { get; init; } = string.Empty;

    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "Origin airport is required.")]
    public int OriginAirportId { get; init; }

    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "Destination airport is required.")]
    public int DestinationAirportId { get; init; }

    [Required]
    public TimeSpan? DepartureTime { get; init; }

    [Required]
    public TimeSpan? ArrivalTime { get; init; }

    [Required]
    public string AircraftType { get; init; } = string.Empty;

    [Required]
    public string DaysOfOperation { get; init; } = string.Empty;

    [Required]
    public DateTime? EffectiveFrom { get; init; }

    public DateTime? EffectiveTo { get; init; }

    [Required]
    public string Status { get; init; } = string.Empty;
}