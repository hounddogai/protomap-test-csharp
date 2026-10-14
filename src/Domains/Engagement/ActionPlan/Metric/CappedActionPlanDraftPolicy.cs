namespace Acme.Workforce.Engagement.ActionPlan.Metric;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of capped action plan draft policy.</summary>
public static class CappedActionPlanDraftPolicy
{
    /// <summary>Puts a capped action plan draft code in its canonical CAPD- form.</summary>
    public static string NormalizeCappedActionPlanDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CAPD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CAPD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeCappedActionPlanDraftLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
