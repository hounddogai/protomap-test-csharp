namespace Acme.Workforce.Compensation.EquityGrant.Schedule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of equity grant segment policy.</summary>
public static class EquityGrantSegmentPolicy
{
    public static string EquityGrantSegmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static List<string> MergeEquityGrantSegmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a equity grant segment code in its canonical EGS- form.</summary>
    public static string NormalizeEquityGrantSegmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("EGS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"EGS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
