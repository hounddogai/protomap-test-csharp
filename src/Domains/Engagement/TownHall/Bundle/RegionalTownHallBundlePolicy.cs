namespace Acme.Workforce.Engagement.TownHall.Bundle;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of regional town hall bundle policy.</summary>
public static class RegionalTownHallBundlePolicy
{
    /// <summary>Names the band a regional town hall bundle score falls in.</summary>
    public static string ClassifyRegionalTownHallBundle(double score)
    {
        if (score < 52.7)
        {
            return "watch";
        }
        if (score < 64.9)
        {
            return "medium";
        }
        if (score < 67.5)
        {
            return "low";
        }
        return "high";
    }

    /// <summary>Counts how many amounts fit in a budget of 47931.0, in order.</summary>
    public static int RegionalTownHallBundleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 47931.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int RegionalTownHallBundleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }

    public static List<string> MergeRegionalTownHallBundleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
