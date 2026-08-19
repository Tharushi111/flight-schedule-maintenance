using System.Data;
using Microsoft.Data.SqlClient;
using ScheduleManagement.Api.Models.Entities;

namespace ScheduleManagement.Api.Repositories;

public sealed class ScheduleRepository : IScheduleRepository
{
    private readonly string _connectionString;

    public ScheduleRepository(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Database connection string 'DefaultConnection' was not found.");
    }

    public async Task<IReadOnlyList<ScheduleListRow>> GetAllAsync(
        int? originAirportId,
        int? destinationAirportId,
        string? status,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                fs.ScheduleId,
                fs.FlightNumber,

                fs.OriginAirportId,
                origin.IataCode AS OriginIataCode,
                origin.City AS OriginCity,

                fs.DestinationAirportId,
                destination.IataCode AS DestinationIataCode,
                destination.City AS DestinationCity,

                fs.DepartureTime,
                fs.ArrivalTime,
                fs.AircraftType,
                fs.DaysOfOperation,
                fs.EffectiveFrom,
                fs.EffectiveTo,
                fs.Status

            FROM FlightSchedule AS fs

            INNER JOIN Airport AS origin
                ON fs.OriginAirportId = origin.AirportId

            INNER JOIN Airport AS destination
                ON fs.DestinationAirportId = destination.AirportId

            WHERE
                (@OriginAirportId IS NULL
                    OR fs.OriginAirportId = @OriginAirportId)

                AND

                (@DestinationAirportId IS NULL
                    OR fs.DestinationAirportId = @DestinationAirportId)

                AND

                (@Status IS NULL
                    OR fs.Status = @Status)

            ORDER BY
                fs.FlightNumber,
                fs.EffectiveFrom;
            """;

        var schedules = new List<ScheduleListRow>();

        await using var connection =
            new SqlConnection(_connectionString);

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.Add(
            "@OriginAirportId",
            SqlDbType.Int).Value =
            originAirportId.HasValue
                ? originAirportId.Value
                : DBNull.Value;

        command.Parameters.Add(
            "@DestinationAirportId",
            SqlDbType.Int).Value =
            destinationAirportId.HasValue
                ? destinationAirportId.Value
                : DBNull.Value;

        command.Parameters.Add(
            "@Status",
            SqlDbType.VarChar,
            12).Value =
            string.IsNullOrWhiteSpace(status)
                ? DBNull.Value
                : status;

        await connection.OpenAsync(cancellationToken);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            schedules.Add(new ScheduleListRow
            {
                ScheduleId =
                    reader.GetInt32(
                        reader.GetOrdinal("ScheduleId")),

                FlightNumber =
                    reader.GetString(
                        reader.GetOrdinal("FlightNumber")),

                OriginAirportId =
                    reader.GetInt32(
                        reader.GetOrdinal("OriginAirportId")),

                OriginIataCode =
                    reader.GetString(
                        reader.GetOrdinal("OriginIataCode")).Trim(),

                OriginCity =
                    reader.GetString(
                        reader.GetOrdinal("OriginCity")),

                DestinationAirportId =
                    reader.GetInt32(
                        reader.GetOrdinal("DestinationAirportId")),

                DestinationIataCode =
                    reader.GetString(
                        reader.GetOrdinal("DestinationIataCode")).Trim(),

                DestinationCity =
                    reader.GetString(
                        reader.GetOrdinal("DestinationCity")),

                DepartureTime =
                    reader.GetTimeSpan(
                        reader.GetOrdinal("DepartureTime")),

                ArrivalTime =
                    reader.GetTimeSpan(
                        reader.GetOrdinal("ArrivalTime")),

                AircraftType =
                    reader.GetString(
                        reader.GetOrdinal("AircraftType")),

                DaysOfOperation =
                    reader.GetString(
                        reader.GetOrdinal("DaysOfOperation")),

                EffectiveFrom =
                    reader.GetDateTime(
                        reader.GetOrdinal("EffectiveFrom")),

                EffectiveTo =
                    reader.IsDBNull(
                        reader.GetOrdinal("EffectiveTo"))
                        ? null
                        : reader.GetDateTime(
                            reader.GetOrdinal("EffectiveTo")),

                Status =
                    reader.GetString(
                        reader.GetOrdinal("Status"))
            });
        }

        return schedules;
    }

    public async Task<int> CreateAsync(
    FlightSchedule schedule,
    CancellationToken cancellationToken = default)
{
    const string sql = """
        INSERT INTO FlightSchedule
        (
            FlightNumber,
            OriginAirportId,
            DestinationAirportId,
            DepartureTime,
            ArrivalTime,
            AircraftType,
            DaysOfOperation,
            EffectiveFrom,
            EffectiveTo,
            Status,
            CreatedOn,
            ModifiedOn
        )
        OUTPUT INSERTED.ScheduleId
        VALUES
        (
            @FlightNumber,
            @OriginAirportId,
            @DestinationAirportId,
            @DepartureTime,
            @ArrivalTime,
            @AircraftType,
            @DaysOfOperation,
            @EffectiveFrom,
            @EffectiveTo,
            @Status,
            @CreatedOn,
            NULL
        );
        """;

    await using var connection =
        new SqlConnection(_connectionString);

    await using var command =
        new SqlCommand(sql, connection);

    command.Parameters.Add(
        "@FlightNumber",
        SqlDbType.VarChar,
        7).Value = schedule.FlightNumber;

    command.Parameters.Add(
        "@OriginAirportId",
        SqlDbType.Int).Value = schedule.OriginAirportId;

    command.Parameters.Add(
        "@DestinationAirportId",
        SqlDbType.Int).Value = schedule.DestinationAirportId;

    command.Parameters.Add(
        "@DepartureTime",
        SqlDbType.Time).Value = schedule.DepartureTime;

    command.Parameters.Add(
        "@ArrivalTime",
        SqlDbType.Time).Value = schedule.ArrivalTime;

    command.Parameters.Add(
        "@AircraftType",
        SqlDbType.VarChar,
        10).Value = schedule.AircraftType;

    command.Parameters.Add(
        "@DaysOfOperation",
        SqlDbType.VarChar,
        7).Value = schedule.DaysOfOperation;

    command.Parameters.Add(
        "@EffectiveFrom",
        SqlDbType.Date).Value = schedule.EffectiveFrom;

    command.Parameters.Add(
        "@EffectiveTo",
        SqlDbType.Date).Value =
        schedule.EffectiveTo.HasValue
            ? schedule.EffectiveTo.Value
            : DBNull.Value;

    command.Parameters.Add(
        "@Status",
        SqlDbType.VarChar,
        12).Value = schedule.Status;

    command.Parameters.Add(
        "@CreatedOn",
        SqlDbType.DateTime2).Value = schedule.CreatedOn;

    await connection.OpenAsync(cancellationToken);

    var result =
        await command.ExecuteScalarAsync(cancellationToken);

    if (result is null)
    {
        throw new InvalidOperationException(
            "The schedule was inserted but no ScheduleId was returned.");
    }

    return Convert.ToInt32(result);
}
}