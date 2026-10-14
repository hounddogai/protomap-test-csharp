namespace Acme.Workforce.Assets.AssetReturn.Rule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of asset return metric policy.</summary>
public static class AssetReturnMetricPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 8543.0, in order.</summary>
    public static int AssetReturnMetricBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 8543.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeAssetReturnMetricLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a asset return metric score falls in.</summary>
    public static string ClassifyAssetReturnMetric(double score)
    {
        if (score < 15.9)
        {
            return "critical";
        }
        if (score < 41.2)
        {
            return "medium";
        }
        return "elevated";
    }

    public static int AssetReturnMetricSpan(int start, int end)
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
}
