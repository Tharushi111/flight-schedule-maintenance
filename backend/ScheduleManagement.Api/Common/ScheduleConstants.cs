namespace ScheduleManagement.Api.Common;

public static class ScheduleConstants
{
    public const string FlightNumberPattern =
        @"^[A-Z]{2}\d{3,4}$";

    public static readonly string[] AllowedAircraftTypes =
    [
        "A320",
        "A330",
        "A350"
    ];

    public static readonly string[] AllowedStatuses =
    [
        "Draft",
        "Published",
        "Suspended"
    ];
}