namespace Acme.Workforce.Contractors.Rate.Proposal;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of rate tier policy.</summary>
public static class RateTierPolicy
{
    public static List<string> MergeRateTierLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a rate tier score falls in.</summary>
    public static string ClassifyRateTier(double score)
    {
        if (score < 3.0)
        {
            return "minimal";
        }
        if (score < 13.6)
        {
            return "watch";
        }
        if (score < 35.9)
        {
            return "critical";
        }
        if (score < 66.1)
        {
            return "ok";
        }
        return "low";
    }
}
