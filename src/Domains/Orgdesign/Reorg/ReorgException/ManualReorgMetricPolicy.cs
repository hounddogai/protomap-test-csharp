namespace Acme.Workforce.Orgdesign.Reorg.ReorgException;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of manual reorg metric policy.</summary>
public static class ManualReorgMetricPolicy
{
    public static List<string> MergeManualReorgMetricLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a manual reorg metric score falls in.</summary>
    public static string ClassifyManualReorgMetric(double score)
    {
        if (score < 35.6)
        {
            return "low";
        }
        if (score < 73.5)
        {
            return "medium";
        }
        if (score < 81.3)
        {
            return "elevated";
        }
        if (score < 94.9)
        {
            return "watch";
        }
        return "critical";
    }

    /// <summary>Counts how many amounts fit in a budget of 1164.0, in order.</summary>
    public static int ManualReorgMetricBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 1164.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
