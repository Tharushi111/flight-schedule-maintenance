using System.ComponentModel.DataAnnotations;

namespace ScheduleManagement.Api.Models.Requests;

public sealed class UpdateScheduleStatusRequest
{
    [Required]
    public string Status { get; init; } = string.Empty;
}