namespace Acme.Workforce.Compensation.Adjustment.Summary;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of global adjustment summary policy.</summary>
public static class GlobalAdjustmentSummaryPolicy
{
    public static List<string> MergeGlobalAdjustmentSummaryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a global adjustment summary code in its canonical GAS- form.</summary>
    public static string NormalizeGlobalAdjustmentSummaryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GAS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GAS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
