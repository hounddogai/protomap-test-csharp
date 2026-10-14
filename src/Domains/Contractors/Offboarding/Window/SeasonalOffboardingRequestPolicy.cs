namespace Acme.Workforce.Contractors.Offboarding.Window;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of seasonal offboarding request policy.</summary>
public static class SeasonalOffboardingRequestPolicy
{
    public static List<string> MergeSeasonalOffboardingRequestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a seasonal offboarding request score falls in.</summary>
    public static string ClassifySeasonalOffboardingRequest(double score)
    {
        if (score < 8.3)
        {
            return "medium";
        }
        if (score < 33.5)
        {
            return "high";
        }
        return "watch";
    }
}
