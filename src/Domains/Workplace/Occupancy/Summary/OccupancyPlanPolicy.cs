namespace Acme.Workforce.Workplace.Occupancy.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of occupancy plan policy.</summary>
public static class OccupancyPlanPolicy
{
    /// <summary>Names the band a occupancy plan score falls in.</summary>
    public static string ClassifyOccupancyPlan(double score)
    {
        if (score < 38.4)
        {
            return "ok";
        }
        if (score < 72.8)
        {
            return "high";
        }
        if (score < 83.6)
        {
            return "minimal";
        }
        return "low";
    }

    /// <summary>Puts a occupancy plan code in its canonical OP- form.</summary>
    public static string NormalizeOccupancyPlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("OP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"OP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double OccupancyPlanWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.69;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
