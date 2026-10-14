namespace Acme.Workforce.Contractors.Rate.Segment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of primary rate request policy.</summary>
public static class PrimaryRateRequestPolicy
{
    public static List<string> MergePrimaryRateRequestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a primary rate request code in its canonical PRR- form.</summary>
    public static string NormalizePrimaryRateRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PRR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PRR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double PrimaryRateRequestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.11;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
