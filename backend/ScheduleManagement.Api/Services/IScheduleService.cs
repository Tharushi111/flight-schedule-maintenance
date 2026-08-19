using ScheduleManagement.Api.Models.Entities;
using ScheduleManagement.Api.Models.Requests;

namespace ScheduleManagement.Api.Services;

public interface IScheduleService
{
    Task<IReadOnlyList<ScheduleListRow>> GetAllAsync(
        int? originAirportId,
        int? destinationAirportId,
        string? status,
        CancellationToken cancellationToken = default);

    Task<int> CreateAsync(
    CreateScheduleRequest request,
    CancellationToken cancellationToken = default);
}