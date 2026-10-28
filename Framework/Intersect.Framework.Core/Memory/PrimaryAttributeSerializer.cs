using System.IO;
using Intersect.Entities;
using Intersect.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Intersect.Utilities;

/// <summary>
/// Reads and writes versioned primary-attribute data. Legacy positional arrays must be
/// converted by the migration path, where their original Stat ordering is explicit.
/// </summary>
public static class PrimaryAttributeSerializer
{
    public const int CurrentSchemaVersion = 1;

    public static string Serialize(PrimaryAttributeValues values)
    {
        ArgumentNullException.ThrowIfNull(values);

        var document = new JObject
        {
            ["schemaVersion"] = CurrentSchemaVersion,
            ["values"] = new JObject
            {
                ["strength"] = values.Strength,
                ["dexterity"] = values.Dexterity,
                ["intelligence"] = values.Intelligence,
                ["agility"] = values.Agility,
                ["wisdom"] = values.Wisdom,
            },
        };

        return document.ToString(Formatting.None);
    }

    public static PrimaryAttributeValues Deserialize(
        string json,
        PrimaryAttributeValues? defaults = null
    )
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new InvalidDataException("Primary attribute data is empty.");
        }

        var document = JToken.Parse(json);
        if (document is not JObject root)
        {
            throw new InvalidDataException(
                "Primary attribute data must be a versioned object. Convert legacy arrays before loading."
            );
        }

        foreach (var property in root.Properties())
        {
            if (property.Name is not ("schemaVersion" or "values"))
            {
                throw new InvalidDataException($"Unknown primary attribute document key '{property.Name}'.");
            }
        }

        var version = root["schemaVersion"];
        if (version == null || version.Type != JTokenType.Integer || version.Value<int>() != CurrentSchemaVersion)
        {
            throw new InvalidDataException(
                $"Unsupported primary attribute schema version '{version?.ToString() ?? "missing"}'."
            );
        }

        if (root["values"] is not JObject values)
        {
            throw new InvalidDataException("Primary attribute data is missing its values object.");
        }

        var result = defaults?.Copy() ?? new PrimaryAttributeValues();
        foreach (var property in values.Properties())
        {
            if (property.Value.Type != JTokenType.Integer)
            {
                throw new InvalidDataException($"Primary attribute '{property.Name}' must be an integer.");
            }

            result[ParseAttribute(property.Name)] = property.Value.Value<int>();
        }

        return result;
    }

    private static PrimaryAttribute ParseAttribute(string key)
    {
        return key switch
        {
            "strength" => PrimaryAttribute.Strength,
            "dexterity" => PrimaryAttribute.Dexterity,
            "intelligence" => PrimaryAttribute.Intelligence,
            "agility" => PrimaryAttribute.Agility,
            "wisdom" => PrimaryAttribute.Wisdom,
            _ => throw new InvalidDataException($"Unknown primary attribute key '{key}'."),
        };
    }
}
