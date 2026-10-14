namespace Acme.Workforce.Talent.Pool.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pool threshold policy.</summary>
public static class PoolThresholdPolicy
{
    /// <summary>Names the band a pool threshold score falls in.</summary>
    public static string ClassifyPoolThreshold(double score)
    {
        if (score < 15.5)
        {
            return "high";
        }
        if (score < 41.0)
        {
            return "critical";
        }
        if (score < 88.6)
        {
            return "minimal";
        }
        return "ok";
    }

    public static double PoolThresholdWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.55;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int PoolThresholdSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Counts how many amounts fit in a budget of 34546.0, in order.</summary>
    public static int PoolThresholdBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 34546.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
