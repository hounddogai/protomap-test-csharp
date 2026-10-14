namespace Acme.Workforce.Absence.ReturnPlan.Threshold;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of capped return plan review policy.</summary>
public static class CappedReturnPlanReviewPolicy
{
    public static List<string> MergeCappedReturnPlanReviewLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a capped return plan review code in its canonical CRPR- form.</summary>
    public static string NormalizeCappedReturnPlanReviewCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CRPR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CRPR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
