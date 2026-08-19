using Microsoft.AspNetCore.Mvc;
using ScheduleManagement.Api.Models.Responses;
using ScheduleManagement.Api.Repositories;

namespace ScheduleManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AirportsController : ControllerBase
{
    private readonly IAirportRepository _airportRepository;

    public AirportsController(
        IAirportRepository airportRepository)
    {
        _airportRepository = airportRepository;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<AirportResponse>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AirportResponse>>>
        GetActiveAirports(
            CancellationToken cancellationToken)
    {
        var airports =
            await _airportRepository.GetActiveAsync(cancellationToken);

        var response = airports
            .Select(airport => new AirportResponse
            {
                AirportId = airport.AirportId,
                IataCode = airport.IataCode,
                AirportName = airport.AirportName,
                City = airport.City,
                CountryCode = airport.CountryCode
            })
            .ToList();

        return Ok(response);
    }
}