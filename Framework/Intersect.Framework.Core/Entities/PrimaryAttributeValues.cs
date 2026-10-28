using Intersect.Enums;

namespace Intersect.Entities;

/// <summary>
/// Values for the five primary attributes, independent from derived combat statistics.
/// </summary>
public sealed class PrimaryAttributeValues
{
    public int Strength { get; set; }

    public int Dexterity { get; set; }

    public int Intelligence { get; set; }

    public int Agility { get; set; }

    public int Wisdom { get; set; }

    public int this[PrimaryAttribute attribute]
    {
        get => attribute switch
        {
            PrimaryAttribute.Strength => Strength,
            PrimaryAttribute.Dexterity => Dexterity,
            PrimaryAttribute.Intelligence => Intelligence,
            PrimaryAttribute.Agility => Agility,
            PrimaryAttribute.Wisdom => Wisdom,
            _ => throw new ArgumentOutOfRangeException(nameof(attribute), attribute, null),
        };
        set
        {
            switch (attribute)
            {
                case PrimaryAttribute.Strength:
                    Strength = value;
                    break;
                case PrimaryAttribute.Dexterity:
                    Dexterity = value;
                    break;
                case PrimaryAttribute.Intelligence:
                    Intelligence = value;
                    break;
                case PrimaryAttribute.Agility:
                    Agility = value;
                    break;
                case PrimaryAttribute.Wisdom:
                    Wisdom = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(attribute), attribute, null);
            }
        }
    }

    public PrimaryAttributeValues Copy()
    {
        return new PrimaryAttributeValues
        {
            Strength = Strength,
            Dexterity = Dexterity,
            Intelligence = Intelligence,
            Agility = Agility,
            Wisdom = Wisdom,
        };
    }
}
