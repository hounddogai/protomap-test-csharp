namespace Acme.Workforce.Compensation.EquityGrant.GrantAssignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of global equity grant assignment policy.</summary>
public static class GlobalEquityGrantAssignmentPolicy
{
    /// <summary>Puts a global equity grant assignment code in its canonical GEGA- form.</summary>
    public static string NormalizeGlobalEquityGrantAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GEGA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GEGA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int GlobalEquityGrantAssignmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }

    public static List<string> MergeGlobalEquityGrantAssignmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
