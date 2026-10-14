namespace Acme.Workforce.Orgdesign.Reorg.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of reorg estimate policy.</summary>
public static class ReorgEstimatePolicy
{
    /// <summary>Names the band a reorg estimate score falls in.</summary>
    public static string ClassifyReorgEstimate(double score)
    {
        if (score < 64.2)
        {
            return "elevated";
        }
        if (score < 86.8)
        {
            return "medium";
        }
        return "watch";
    }

    public static double ReorgEstimateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.94;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
