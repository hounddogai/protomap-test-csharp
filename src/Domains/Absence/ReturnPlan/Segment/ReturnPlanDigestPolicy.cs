namespace Acme.Workforce.Absence.ReturnPlan.Segment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of return plan digest policy.</summary>
public static class ReturnPlanDigestPolicy
{
    public static List<string> MergeReturnPlanDigestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a return plan digest code in its canonical RPD- form.</summary>
    public static string NormalizeReturnPlanDigestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
