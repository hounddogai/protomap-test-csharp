namespace Acme.Workforce.Talent.Mobility.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of mobility bundle policy.</summary>
public static class MobilityBundlePolicy
{
    public static int MobilityBundleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }

    public static double MobilityBundleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.41;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 89193.0, in order.</summary>
    public static int MobilityBundleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 89193.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
