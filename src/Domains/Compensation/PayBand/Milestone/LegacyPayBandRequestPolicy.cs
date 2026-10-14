namespace Acme.Workforce.Compensation.PayBand.Milestone;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of legacy pay band request policy.</summary>
public static class LegacyPayBandRequestPolicy
{
    public static double LegacyPayBandRequestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.0;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string LegacyPayBandRequestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static List<string> MergeLegacyPayBandRequestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a legacy pay band request code in its canonical LPBR- form.</summary>
    public static string NormalizeLegacyPayBandRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LPBR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LPBR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
