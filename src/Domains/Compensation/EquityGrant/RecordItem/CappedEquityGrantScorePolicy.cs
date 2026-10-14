namespace Acme.Workforce.Compensation.EquityGrant.RecordItem;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of capped equity grant score policy.</summary>
public static class CappedEquityGrantScorePolicy
{
    /// <summary>Puts a capped equity grant score code in its canonical CEGS- form.</summary>
    public static string NormalizeCappedEquityGrantScoreCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CEGS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CEGS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeCappedEquityGrantScoreLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int CappedEquityGrantScoreSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    public static double CappedEquityGrantScoreWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.44;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
