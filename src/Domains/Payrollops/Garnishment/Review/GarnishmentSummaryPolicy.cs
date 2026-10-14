namespace Acme.Workforce.Payrollops.Garnishment.Review;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of garnishment summary policy.</summary>
public static class GarnishmentSummaryPolicy
{
    public static string GarnishmentSummaryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a garnishment summary code in its canonical GS- form.</summary>
    public static string NormalizeGarnishmentSummaryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeGarnishmentSummaryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int GarnishmentSummarySpan(int start, int end)
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
}
