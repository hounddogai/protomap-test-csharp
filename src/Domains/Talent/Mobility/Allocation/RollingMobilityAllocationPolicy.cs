namespace Acme.Workforce.Talent.Mobility.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rolling mobility allocation policy.</summary>
public static class RollingMobilityAllocationPolicy
{
    /// <summary>Names the band a rolling mobility allocation score falls in.</summary>
    public static string ClassifyRollingMobilityAllocation(double score)
    {
        if (score < 12.1)
        {
            return "watch";
        }
        if (score < 75.4)
        {
            return "low";
        }
        return "ok";
    }

    /// <summary>Counts how many amounts fit in a budget of 82906.0, in order.</summary>
    public static int RollingMobilityAllocationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 82906.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double RollingMobilityAllocationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.36;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int RollingMobilityAllocationSpan(int start, int end)
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
}
