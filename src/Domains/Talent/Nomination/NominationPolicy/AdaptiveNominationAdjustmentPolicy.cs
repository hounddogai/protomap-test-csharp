namespace Acme.Workforce.Talent.Nomination.NominationPolicy;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of adaptive nomination adjustment policy.</summary>
public static class AdaptiveNominationAdjustmentPolicy
{
    /// <summary>Names the band a adaptive nomination adjustment score falls in.</summary>
    public static string ClassifyAdaptiveNominationAdjustment(double score)
    {
        if (score < 4.3)
        {
            return "elevated";
        }
        if (score < 42.5)
        {
            return "high";
        }
        if (score < 57.3)
        {
            return "medium";
        }
        if (score < 75.5)
        {
            return "ok";
        }
        return "low";
    }

    public static List<string> MergeAdaptiveNominationAdjustmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
