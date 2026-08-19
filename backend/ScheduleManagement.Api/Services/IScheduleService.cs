using ScheduleManagement.Api.Models.Entities;

namespace ScheduleManagement.Api.Services;

public interface IScheduleService
{
    Task<IReadOnlyList<ScheduleListRow>> GetAllAsync(
        int? originAirportId,
        int? destinationAirportId,
        string? status,
        CancellationToken cancellationToken = default);
}