namespace Acme.Workforce.Orgdesign.Headcount.Slot;

/// <summary>Functions of tiered headcount estimate policy.</summary>
public static class TieredHeadcountEstimatePolicy
{
    public static int TieredHeadcountEstimateSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Names the band a tiered headcount estimate score falls in.</summary>
    public static string ClassifyTieredHeadcountEstimate(double score)
    {
        if (score < 52.8)
        {
            return "elevated";
        }
        if (score < 53.5)
        {
            return "medium";
        }
        if (score < 80.1)
        {
            return "low";
        }
        if (score < 97.2)
        {
            return "high";
        }
        return "minimal";
    }
}
