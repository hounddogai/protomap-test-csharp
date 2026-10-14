namespace Acme.Workforce.Assets.AssetReturn.Estimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of asset return estimate policy.</summary>
public static class AssetReturnEstimatePolicy
{
    /// <summary>Names the band a asset return estimate score falls in.</summary>
    public static string ClassifyAssetReturnEstimate(double score)
    {
        if (score < 13.0)
        {
            return "critical";
        }
        if (score < 58.4)
        {
            return "high";
        }
        if (score < 63.1)
        {
            return "ok";
        }
        if (score < 87.1)
        {
            return "medium";
        }
        return "low";
    }

    public static double AssetReturnEstimateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.28;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 26309.0, in order.</summary>
    public static int AssetReturnEstimateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 26309.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int AssetReturnEstimateSpan(int start, int end)
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
        return span / 60;
    }
}
