using System.Data;
using Microsoft.Data.SqlClient;
using ScheduleManagement.Api.Models.Entities;

namespace ScheduleManagement.Api.Repositories;

public sealed class AirportRepository : IAirportRepository
{
    private readonly string _connectionString;

    public AirportRepository(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Database connection string 'DefaultConnection' was not found.");
    }

    public async Task<IReadOnlyList<Airport>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                AirportId,
                IataCode,
                AirportName,
                City,
                CountryCode,
                IsActive
            FROM Airport
            WHERE IsActive = 1
            ORDER BY IataCode;
            """;

        var airports = new List<Airport>();

        await using var connection =
            new SqlConnection(_connectionString);

        await using var command =
            new SqlCommand(sql, connection);

        await connection.OpenAsync(cancellationToken);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        var airportIdOrdinal =
            reader.GetOrdinal("AirportId");

        var iataCodeOrdinal =
            reader.GetOrdinal("IataCode");

        var airportNameOrdinal =
            reader.GetOrdinal("AirportName");

        var cityOrdinal =
            reader.GetOrdinal("City");

        var countryCodeOrdinal =
            reader.GetOrdinal("CountryCode");

        var isActiveOrdinal =
            reader.GetOrdinal("IsActive");

        while (await reader.ReadAsync(cancellationToken))
        {
            airports.Add(new Airport
            {
                AirportId =
                    reader.GetInt32(airportIdOrdinal),

                IataCode =
                    reader.GetString(iataCodeOrdinal).Trim(),

                AirportName =
                    reader.GetString(airportNameOrdinal),

                City =
                    reader.GetString(cityOrdinal),

                CountryCode =
                    reader.GetString(countryCodeOrdinal).Trim(),

                IsActive =
                    reader.GetBoolean(isActiveOrdinal)
            });
        }

        return airports;
    }

    public async Task<bool> ExistsAsync(
    int airportId,
    CancellationToken cancellationToken = default)
{
    const string sql = """
        SELECT COUNT(1)
        FROM Airport
        WHERE AirportId = @AirportId;
        """;

    await using var connection =
        new SqlConnection(_connectionString);

    await using var command =
        new SqlCommand(sql, connection);

    command.Parameters.Add(
        "@AirportId",
        SqlDbType.Int).Value = airportId;

    await connection.OpenAsync(cancellationToken);

    var result =
        await command.ExecuteScalarAsync(cancellationToken);

    return Convert.ToInt32(result) > 0;
}
}