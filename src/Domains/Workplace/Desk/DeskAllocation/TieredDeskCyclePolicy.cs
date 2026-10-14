namespace Acme.Workforce.Workplace.Desk.DeskAllocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tiered desk cycle policy.</summary>
public static class TieredDeskCyclePolicy
{
    /// <summary>Names the band a tiered desk cycle score falls in.</summary>
    public static string ClassifyTieredDeskCycle(double score)
    {
        if (score < 27.1)
        {
            return "high";
        }
        if (score < 49.1)
        {
            return "medium";
        }
        if (score < 49.4)
        {
            return "minimal";
        }
        return "ok";
    }

    public static string TieredDeskCycleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
