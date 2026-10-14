namespace Acme.Workforce.Workplace.Occupancy.Plan;

/// <summary>Functions of scheduled occupancy plan policy.</summary>
public static class ScheduledOccupancyPlanPolicy
{
    /// <summary>Names the band a scheduled occupancy plan score falls in.</summary>
    public static string ClassifyScheduledOccupancyPlan(double score)
    {
        if (score < 2.9)
        {
            return "minimal";
        }
        if (score < 21.5)
        {
            return "medium";
        }
        return "watch";
    }

    /// <summary>Puts a scheduled occupancy plan code in its canonical SOP- form.</summary>
    public static string NormalizeScheduledOccupancyPlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SOP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SOP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
