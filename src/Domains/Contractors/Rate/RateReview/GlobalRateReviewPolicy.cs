namespace Acme.Workforce.Contractors.Rate.RateReview;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of global rate review policy.</summary>
public static class GlobalRateReviewPolicy
{
    /// <summary>Names the band a global rate review score falls in.</summary>
    public static string ClassifyGlobalRateReview(double score)
    {
        if (score < 22.6)
        {
            return "low";
        }
        if (score < 63.5)
        {
            return "watch";
        }
        return "ok";
    }

    public static List<string> MergeGlobalRateReviewLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
