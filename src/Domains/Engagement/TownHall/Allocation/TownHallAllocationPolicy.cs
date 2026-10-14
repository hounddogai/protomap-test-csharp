namespace Acme.Workforce.Engagement.TownHall.Allocation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of town hall allocation policy.</summary>
public static class TownHallAllocationPolicy
{
    public static List<string> MergeTownHallAllocationLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int TownHallAllocationSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Counts how many amounts fit in a budget of 80585.0, in order.</summary>
    public static int TownHallAllocationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 80585.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a town hall allocation score falls in.</summary>
    public static string ClassifyTownHallAllocation(double score)
    {
        if (score < 6.7)
        {
            return "critical";
        }
        if (score < 93.0)
        {
            return "minimal";
        }
        if (score < 98.7)
        {
            return "ok";
        }
        return "elevated";
    }
}
