using System.IO;
using Intersect.Entities;
using Intersect.Enums;
using Newtonsoft.Json.Linq;

namespace Intersect.Utilities;

/// <summary>
/// Result of converting a legacy stat allocation array. Defense allocations are
/// returned to the unspent pool because defense remains a derived combat stat.
/// </summary>
public sealed record LegacyPrimaryAttributeAllocationConversion(
    PrimaryAttributeValues Allocations,
    int UnspentStatPoints,
    int ReturnedDefensePoints
);

/// <summary>
/// Converts the v0 Intersect stat array using the legacy Stat enum's documented
/// positions. This converter is explicit and is not called by normal loading.
/// </summary>
public static class LegacyPrimaryAttributeConverter
{
    private const int LegacyStatCount = 5;

    /// <summary>
    /// Converts legacy base stats. Dexterity is taken from the migrated class defaults;
    /// legacy Defense is intentionally not treated as a primary attribute.
    /// </summary>
    public static PrimaryAttributeValues ConvertBaseStats(
        string legacyJson,
        PrimaryAttributeValues classDefaults
    )
    {
        ArgumentNullException.ThrowIfNull(classDefaults);

        var legacyValues = ParseLegacyArray(legacyJson);
        var result = classDefaults.Copy();
        result[PrimaryAttribute.Strength] = legacyValues[(int)Stat.Attack];
        result[PrimaryAttribute.Intelligence] = legacyValues[(int)Stat.AbilityPower];
        result[PrimaryAttribute.Agility] = legacyValues[(int)Stat.Speed];
        result[PrimaryAttribute.Wisdom] = legacyValues[(int)Stat.MagicResist];

        return result;
    }

    /// <summary>
    /// Converts legacy allocations and refunds Defense points to the unspent pool.
    /// </summary>
    public static LegacyPrimaryAttributeAllocationConversion ConvertAllocations(
        string legacyJson,
        int unspentStatPoints
    )
    {
        if (unspentStatPoints < 0)
        {
            throw new InvalidDataException("Unspent stat points cannot be negative.");
        }

        var legacyValues = ParseLegacyArray(legacyJson);
        var returnedDefensePoints = legacyValues[(int)Stat.Defense];
        var newUnspentPoints = checked(unspentStatPoints + returnedDefensePoints);
        var allocations = new PrimaryAttributeValues
        {
            Strength = legacyValues[(int)Stat.Attack],
            Intelligence = legacyValues[(int)Stat.AbilityPower],
            Agility = legacyValues[(int)Stat.Speed],
            Wisdom = legacyValues[(int)Stat.MagicResist],
        };

        return new LegacyPrimaryAttributeAllocationConversion(
            allocations,
            newUnspentPoints,
            returnedDefensePoints
        );
    }

    private static int[] ParseLegacyArray(string legacyJson)
    {
        if (string.IsNullOrWhiteSpace(legacyJson))
        {
            throw new InvalidDataException("Legacy stat data is empty.");
        }

        if (JToken.Parse(legacyJson) is not JArray array || array.Count != LegacyStatCount)
        {
            throw new InvalidDataException(
                $"Legacy stat data must be an array of exactly {LegacyStatCount} integers."
            );
        }

        var values = new int[LegacyStatCount];
        for (var index = 0; index < array.Count; index++)
        {
            var token = array[index];
            if (token?.Type != JTokenType.Integer)
            {
                throw new InvalidDataException($"Legacy stat value at index {index} must be an integer.");
            }

            var value = token.Value<int>();
            if (value < 0)
            {
                throw new InvalidDataException($"Legacy stat value at index {index} cannot be negative.");
            }

            values[index] = value;
        }

        return values;
    }
}
