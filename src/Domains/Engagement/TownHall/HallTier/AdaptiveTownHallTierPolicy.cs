namespace Acme.Workforce.Engagement.TownHall.HallTier;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of adaptive town hall tier policy.</summary>
public static class AdaptiveTownHallTierPolicy
{
    /// <summary>Names the band a adaptive town hall tier score falls in.</summary>
    public static string ClassifyAdaptiveTownHallTier(double score)
    {
        if (score < 41.1)
        {
            return "ok";
        }
        if (score < 55.2)
        {
            return "elevated";
        }
        if (score < 75.6)
        {
            return "critical";
        }
        return "watch";
    }

    /// <summary>Counts how many amounts fit in a budget of 33859.0, in order.</summary>
    public static int AdaptiveTownHallTierBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 33859.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeAdaptiveTownHallTierLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
