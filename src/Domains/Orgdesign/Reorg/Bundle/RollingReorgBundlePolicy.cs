namespace Acme.Workforce.Orgdesign.Reorg.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rolling reorg bundle policy.</summary>
public static class RollingReorgBundlePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 33449.0, in order.</summary>
    public static int RollingReorgBundleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 33449.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a rolling reorg bundle score falls in.</summary>
    public static string ClassifyRollingReorgBundle(double score)
    {
        if (score < 9.1)
        {
            return "critical";
        }
        if (score < 15.3)
        {
            return "high";
        }
        if (score < 15.5)
        {
            return "minimal";
        }
        return "watch";
    }

    public static double RollingReorgBundleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.26;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string RollingReorgBundleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
