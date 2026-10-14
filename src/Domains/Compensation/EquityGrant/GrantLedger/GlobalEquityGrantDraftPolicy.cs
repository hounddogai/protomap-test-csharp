namespace Acme.Workforce.Compensation.EquityGrant.GrantLedger;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of global equity grant draft policy.</summary>
public static class GlobalEquityGrantDraftPolicy
{
    public static List<string> MergeGlobalEquityGrantDraftLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int GlobalEquityGrantDraftSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Puts a global equity grant draft code in its canonical GEGD- form.</summary>
    public static string NormalizeGlobalEquityGrantDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GEGD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GEGD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double GlobalEquityGrantDraftWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.82;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
