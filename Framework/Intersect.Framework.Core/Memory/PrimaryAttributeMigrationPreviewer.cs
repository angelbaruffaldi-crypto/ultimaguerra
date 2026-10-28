using System.IO;
using Intersect.Entities;
using Newtonsoft.Json;

namespace Intersect.Utilities;

/// <summary>
/// Read-only input for previewing one legacy player conversion.
/// </summary>
public sealed record LegacyPlayerAttributeMigrationInput(
    Guid PlayerId,
    string? LegacyStatsJson,
    string? LegacyStatPointsJson,
    int UnspentStatPoints,
    PrimaryAttributeValues? ClassDefaults
);

/// <summary>
/// A conversion preview that retains the exact source fields for later backup.
/// A row is eligible only when both legacy arrays and class defaults were valid.
/// </summary>
public sealed record PlayerAttributeMigrationPreviewRow(
    Guid PlayerId,
    string? LegacyStatsJson,
    string? LegacyStatPointsJson,
    int? OriginalUnspentStatPoints,
    string? PrimaryAttributesJson,
    string? AttributePointAllocationsJson,
    int? ConvertedUnspentStatPoints,
    int? ReturnedDefensePoints,
    string? Error
)
{
    public bool CanMigrate => Error == null;
}

/// <summary>
/// Aggregate result for a read-only migration preview.
/// </summary>
public sealed record PrimaryAttributeMigrationPreviewReport(
    IReadOnlyList<PlayerAttributeMigrationPreviewRow> Rows
)
{
    public int ReadyCount => Rows.Count(row => row.CanMigrate);

    public int ErrorCount => Rows.Count(row => !row.CanMigrate);
}

/// <summary>
/// Produces conversion output for review without opening or changing a database.
/// </summary>
public static class PrimaryAttributeMigrationPreviewer
{
    public static PrimaryAttributeMigrationPreviewReport Build(
        IEnumerable<LegacyPlayerAttributeMigrationInput> players
    )
    {
        ArgumentNullException.ThrowIfNull(players);

        var rows = players.Select(CreateRow).ToArray();
        return new PrimaryAttributeMigrationPreviewReport(rows);
    }

    private static PlayerAttributeMigrationPreviewRow CreateRow(
        LegacyPlayerAttributeMigrationInput player
    )
    {
        if (player == null)
        {
            return new PlayerAttributeMigrationPreviewRow(
                Guid.Empty,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                "Migration input row is null."
            );
        }

        if (player.PlayerId == Guid.Empty)
        {
            return ErrorRow(player, "Player ID is empty.");
        }

        if (player.ClassDefaults == null)
        {
            return ErrorRow(player, "Class attribute defaults are missing.");
        }

        try
        {
            var primaryAttributes = LegacyPrimaryAttributeConverter.ConvertBaseStats(
                player.LegacyStatsJson!,
                player.ClassDefaults
            );
            var allocationConversion = LegacyPrimaryAttributeConverter.ConvertAllocations(
                player.LegacyStatPointsJson!,
                player.UnspentStatPoints
            );

            return new PlayerAttributeMigrationPreviewRow(
                player.PlayerId,
                player.LegacyStatsJson,
                player.LegacyStatPointsJson,
                player.UnspentStatPoints,
                PrimaryAttributeSerializer.Serialize(primaryAttributes),
                PrimaryAttributeSerializer.Serialize(allocationConversion.Allocations),
                allocationConversion.UnspentStatPoints,
                allocationConversion.ReturnedDefensePoints,
                null
            );
        }
        catch (Exception exception) when (
            exception is InvalidDataException or JsonException or OverflowException
        )
        {
            return ErrorRow(player, exception.Message);
        }
    }

    private static PlayerAttributeMigrationPreviewRow ErrorRow(
        LegacyPlayerAttributeMigrationInput player,
        string error
    )
    {
        return new PlayerAttributeMigrationPreviewRow(
            player.PlayerId,
            player.LegacyStatsJson,
            player.LegacyStatPointsJson,
            player.UnspentStatPoints,
            null,
            null,
            null,
            null,
            error
        );
    }
}
