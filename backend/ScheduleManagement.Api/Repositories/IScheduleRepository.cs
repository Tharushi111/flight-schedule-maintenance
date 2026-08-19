using ScheduleManagement.Api.Models.Entities;

namespace ScheduleManagement.Api.Repositories;

public interface IScheduleRepository
{
    Task<IReadOnlyList<ScheduleListRow>> GetAllAsync(
        int? originAirportId,
        int? destinationAirportId,
        string? status,
        CancellationToken cancellationToken = default);

    Task<int> CreateAsync(
    FlightSchedule schedule,
    CancellationToken cancellationToken = default);

    Task<FlightSchedule?> GetByIdAsync(
    int scheduleId,
    CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(
    FlightSchedule schedule,
    CancellationToken cancellationToken = default);

    Task<bool> UpdateStatusAsync(
    int scheduleId,
    string status,
    DateTime modifiedOn,
    CancellationToken cancellationToken = default);
}