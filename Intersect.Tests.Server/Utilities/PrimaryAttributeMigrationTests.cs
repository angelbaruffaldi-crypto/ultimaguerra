using System.IO;
using Intersect.Entities;
using Intersect.Enums;
using Intersect.Utilities;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Intersect.Tests.Server.Utilities;

[TestFixture]
public sealed class PrimaryAttributeMigrationTests
{
    [Test]
    public void SerializerWritesStableVersionedKeysAndRoundTrips()
    {
        var values = new PrimaryAttributeValues
        {
            Strength = 11,
            Dexterity = 12,
            Intelligence = 13,
            Agility = 14,
            Wisdom = 15,
        };

        var json = PrimaryAttributeSerializer.Serialize(values);
        var restored = PrimaryAttributeSerializer.Deserialize(json);

        Assert.That(
            json,
            Is.EqualTo(
                "{\"schemaVersion\":1,\"values\":{\"strength\":11,\"dexterity\":12,\"intelligence\":13,\"agility\":14,\"wisdom\":15}}"
            )
        );
        Assert.That(restored.Strength, Is.EqualTo(11));
        Assert.That(restored.Dexterity, Is.EqualTo(12));
        Assert.That(restored.Intelligence, Is.EqualTo(13));
        Assert.That(restored.Agility, Is.EqualTo(14));
        Assert.That(restored.Wisdom, Is.EqualTo(15));
    }

    [Test]
    public void SerializerUsesDescriptorDefaultsForMissingKeys()
    {
        var defaults = new PrimaryAttributeValues { Dexterity = 7, Wisdom = 8 };

        var restored = PrimaryAttributeSerializer.Deserialize(
            "{\"schemaVersion\":1,\"values\":{\"strength\":12}}",
            defaults
        );

        Assert.That(restored.Strength, Is.EqualTo(12));
        Assert.That(restored.Dexterity, Is.EqualTo(7));
        Assert.That(restored.Wisdom, Is.EqualTo(8));
    }

    [Test]
    public void SerializerRejectsMissingKeysWithoutDescriptorDefaults()
    {
        Assert.Throws<InvalidDataException>(() =>
            PrimaryAttributeSerializer.Deserialize("{\"schemaVersion\":1,\"values\":{\"strength\":12}}")
        );
    }

    [Test]
    public void SerializerRejectsDuplicateKeysAndNegativeValues()
    {
        Assert.Throws<JsonReaderException>(() =>
            PrimaryAttributeSerializer.Deserialize(
                "{\"schemaVersion\":1,\"values\":{\"strength\":1,\"strength\":2}}"
            )
        );

        Assert.Throws<InvalidDataException>(() =>
            PrimaryAttributeSerializer.Deserialize(
                "{\"schemaVersion\":1,\"values\":{\"strength\":-1,\"dexterity\":0,\"intelligence\":0,\"agility\":0,\"wisdom\":0}}"
            )
        );
    }

    [Test]
    public void LegacyBaseStatsMapByNamedLegacyStatAndKeepClassDexterity()
    {
        var classDefaults = new PrimaryAttributeValues { Dexterity = 9 };

        var converted = LegacyPrimaryAttributeConverter.ConvertBaseStats(
            "[10,20,30,40,50]",
            classDefaults
        );

        Assert.That(converted[PrimaryAttribute.Strength], Is.EqualTo(10));
        Assert.That(converted[PrimaryAttribute.Dexterity], Is.EqualTo(9));
        Assert.That(converted[PrimaryAttribute.Intelligence], Is.EqualTo(20));
        Assert.That(converted[PrimaryAttribute.Agility], Is.EqualTo(50));
        Assert.That(converted[PrimaryAttribute.Wisdom], Is.EqualTo(40));
    }

    [Test]
    public void LegacyAllocationsRefundDefenseToUnspentPoints()
    {
        var converted = LegacyPrimaryAttributeConverter.ConvertAllocations("[1,2,3,4,5]", 10);

        Assert.That(converted.Allocations.Strength, Is.EqualTo(1));
        Assert.That(converted.Allocations.Intelligence, Is.EqualTo(2));
        Assert.That(converted.Allocations.Agility, Is.EqualTo(5));
        Assert.That(converted.Allocations.Wisdom, Is.EqualTo(4));
        Assert.That(converted.ReturnedDefensePoints, Is.EqualTo(3));
        Assert.That(converted.UnspentStatPoints, Is.EqualTo(13));
    }

    [Test]
    public void PreviewRetainsLegacySourceAndReportsInvalidRows()
    {
        var playerId = Guid.NewGuid();
        var legacyBase = "[10,20,30,40,50]";
        var legacyAllocations = "[1,2,3,4,5]";
        var report = PrimaryAttributeMigrationPreviewer.Build(
        [
            new LegacyPlayerAttributeMigrationInput(
                playerId,
                legacyBase,
                legacyAllocations,
                10,
                new PrimaryAttributeValues { Dexterity = 9 }
            ),
            new LegacyPlayerAttributeMigrationInput(
                Guid.NewGuid(),
                "not-json",
                legacyAllocations,
                0,
                new PrimaryAttributeValues()
            ),
        ]
        );

        Assert.That(report.ReadyCount, Is.EqualTo(1));
        Assert.That(report.ErrorCount, Is.EqualTo(1));

        var convertedRow = report.Rows[0];
        Assert.That(convertedRow.PlayerId, Is.EqualTo(playerId));
        Assert.That(convertedRow.LegacyStatsJson, Is.EqualTo(legacyBase));
        Assert.That(convertedRow.LegacyStatPointsJson, Is.EqualTo(legacyAllocations));
        Assert.That(convertedRow.ConvertedUnspentStatPoints, Is.EqualTo(13));
        Assert.That(convertedRow.ReturnedDefensePoints, Is.EqualTo(3));
        Assert.That(
            PrimaryAttributeSerializer.Deserialize(convertedRow.PrimaryAttributesJson!).Dexterity,
            Is.EqualTo(9)
        );

        Assert.That(report.Rows[1].CanMigrate, Is.False);
        Assert.That(report.Rows[1].PrimaryAttributesJson, Is.Null);
        Assert.That(report.Rows[1].Error, Is.Not.Empty);
    }
}
