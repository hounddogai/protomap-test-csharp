namespace Acme.Workforce.Talent.Pool.PoolAssignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of legacy pool segment policy.</summary>
public static class LegacyPoolSegmentPolicy
{
    public static List<string> MergeLegacyPoolSegmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a legacy pool segment code in its canonical LPS- form.</summary>
    public static string NormalizeLegacyPoolSegmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LPS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LPS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string LegacyPoolSegmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
