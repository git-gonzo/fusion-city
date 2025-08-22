public enum AttributeType
{
    Agility,
    Balance,
    Endurance,
    Health,
    Strength,
    Determination,
    Imagination,
    Intelligence,
    Memory,
    Wisdom,
    Curiosity,
    Empathy,
    Humor,
    Loyalty,
    Sympathy
}

[System.Serializable]
public class RewardAttribute
{
    public SO_Attribute attribute;
    public float amount;
}