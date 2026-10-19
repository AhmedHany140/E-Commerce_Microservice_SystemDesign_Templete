using Microsoft.Data.SqlClient;

namespace ECommerce.Shared.Persistence;

public static class WolverineSchemaRepair
{
    public static async Task NormalizeReceivedAtAsync(
        string? connectionString,
        string schema,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("A SQL connection string is required for Wolverine persistence.");

        if (schema.Any(character => !char.IsLetterOrDigit(character) && character != '_'))
            throw new ArgumentException("The Wolverine schema name contains unsupported characters.", nameof(schema));

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            DECLARE @receivedAtDefault sysname, @dropDefaultSql nvarchar(512);
            SELECT TOP (1) @receivedAtDefault = dc.name
            FROM sys.default_constraints dc
            INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
            INNER JOIN sys.tables t ON t.object_id = c.object_id
            INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
            WHERE s.name = N'{schema}'
              AND t.name = N'wolverine_incoming_envelopes'
              AND c.name = N'received_at';

            IF @receivedAtDefault IS NOT NULL
            BEGIN
                SET @dropDefaultSql = N'ALTER TABLE [{schema}].wolverine_incoming_envelopes DROP CONSTRAINT ' + QUOTENAME(@receivedAtDefault);
                EXEC(@dropDefaultSql);
            END;

            IF COL_LENGTH(N'{schema}.wolverine_incoming_envelopes', N'received_at') IS NOT NULL
            BEGIN
                ALTER TABLE [{schema}].wolverine_incoming_envelopes
                ALTER COLUMN received_at varchar(250) NULL;
            END;
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
