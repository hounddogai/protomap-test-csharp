namespace Acme.Workforce.Compensation.EquityGrant.Batch;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of equity grant review policy.</summary>
public static class EquityGrantReviewPolicy
{
    /// <summary>Names the band a equity grant review score falls in.</summary>
    public static string ClassifyEquityGrantReview(double score)
    {
        if (score < 12.1)
        {
            return "critical";
        }
        if (score < 60.5)
        {
            return "high";
        }
        return "low";
    }

    public static double EquityGrantReviewWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.7;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeEquityGrantReviewLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
