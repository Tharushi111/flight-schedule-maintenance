using ScheduleManagement.Api.Models.Entities;

namespace ScheduleManagement.Api.Repositories;

public interface IScheduleRepository
{
    Task<IReadOnlyList<ScheduleListRow>> GetAllAsync(
        int? originAirportId,
        int? destinationAirportId,
        string? status,
        CancellationToken cancellationToken = default);
}