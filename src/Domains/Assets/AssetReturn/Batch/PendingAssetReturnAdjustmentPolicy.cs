namespace Acme.Workforce.Assets.AssetReturn.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pending asset return adjustment policy.</summary>
public static class PendingAssetReturnAdjustmentPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 63461.0, in order.</summary>
    public static int PendingAssetReturnAdjustmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 63461.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a pending asset return adjustment score falls in.</summary>
    public static string ClassifyPendingAssetReturnAdjustment(double score)
    {
        if (score < 8.2)
        {
            return "elevated";
        }
        if (score < 20.1)
        {
            return "ok";
        }
        if (score < 70.0)
        {
            return "minimal";
        }
        if (score < 93.6)
        {
            return "critical";
        }
        return "high";
    }

    public static double PendingAssetReturnAdjustmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.38;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int PendingAssetReturnAdjustmentSpan(int start, int end)
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
