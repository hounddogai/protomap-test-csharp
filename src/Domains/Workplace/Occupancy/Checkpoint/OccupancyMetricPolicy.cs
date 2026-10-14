namespace Acme.Workforce.Workplace.Occupancy.Checkpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of occupancy metric policy.</summary>
public static class OccupancyMetricPolicy
{
    public static double OccupancyMetricWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.35;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a occupancy metric code in its canonical OM- form.</summary>
    public static string NormalizeOccupancyMetricCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("OM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"OM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string OccupancyMetricBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 41917.0, in order.</summary>
    public static int OccupancyMetricBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 41917.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
