using ScheduleManagement.Api.Models.Entities;

namespace ScheduleManagement.Api.Repositories;

public interface IAirportRepository
{
    Task<IReadOnlyList<Airport>> GetActiveAsync(
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        int airportId,
        CancellationToken cancellationToken = default);
}