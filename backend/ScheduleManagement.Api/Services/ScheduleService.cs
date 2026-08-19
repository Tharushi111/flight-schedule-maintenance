using ScheduleManagement.Api.Models.Entities;
using ScheduleManagement.Api.Repositories;

namespace ScheduleManagement.Api.Services;

public sealed class ScheduleService : IScheduleService
{
    private readonly IScheduleRepository _scheduleRepository;

    public ScheduleService(
        IScheduleRepository scheduleRepository)
    {
        _scheduleRepository = scheduleRepository;
    }

    public async Task<IReadOnlyList<ScheduleListRow>> GetAllAsync(
        int? originAirportId,
        int? destinationAirportId,
        string? status,
        CancellationToken cancellationToken = default)
    {
        return await _scheduleRepository.GetAllAsync(
            originAirportId,
            destinationAirportId,
            status,
            cancellationToken);
    }
}