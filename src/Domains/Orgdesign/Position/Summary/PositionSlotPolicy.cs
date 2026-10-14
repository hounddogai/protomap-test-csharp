namespace Acme.Workforce.Orgdesign.Position.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of position slot policy.</summary>
public static class PositionSlotPolicy
{
    /// <summary>Names the band a position slot score falls in.</summary>
    public static string ClassifyPositionSlot(double score)
    {
        if (score < 47.1)
        {
            return "low";
        }
        if (score < 63.3)
        {
            return "minimal";
        }
        if (score < 91.4)
        {
            return "medium";
        }
        return "ok";
    }

    public static string PositionSlotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
