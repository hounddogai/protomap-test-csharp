namespace Acme.Workforce.Workplace.Occupancy.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of regional occupancy digest policy.</summary>
public static class RegionalOccupancyDigestPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 71558.0, in order.</summary>
    public static int RegionalOccupancyDigestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 71558.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string RegionalOccupancyDigestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a regional occupancy digest score falls in.</summary>
    public static string ClassifyRegionalOccupancyDigest(double score)
    {
        if (score < 13.3)
        {
            return "ok";
        }
        if (score < 29.7)
        {
            return "low";
        }
        if (score < 70.3)
        {
            return "critical";
        }
        if (score < 80.4)
        {
            return "high";
        }
        return "watch";
    }

    public static double RegionalOccupancyDigestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.81;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
