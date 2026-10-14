namespace Acme.Workforce.Assets.AssetReturn.Adjustment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of asset return adjustment policy.</summary>
public static class AssetReturnAdjustmentPolicy
{
    /// <summary>Names the band a asset return adjustment score falls in.</summary>
    public static string ClassifyAssetReturnAdjustment(double score)
    {
        if (score < 25.1)
        {
            return "high";
        }
        if (score < 28.6)
        {
            return "ok";
        }
        if (score < 45.9)
        {
            return "critical";
        }
        if (score < 45.9)
        {
            return "elevated";
        }
        return "watch";
    }

    public static double AssetReturnAdjustmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.24;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 79096.0, in order.</summary>
    public static int AssetReturnAdjustmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 79096.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeAssetReturnAdjustmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
