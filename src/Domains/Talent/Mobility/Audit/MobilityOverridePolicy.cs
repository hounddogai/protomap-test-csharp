namespace Acme.Workforce.Talent.Mobility.Audit;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of mobility override policy.</summary>
public static class MobilityOverridePolicy
{
    public static string MobilityOverrideBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a mobility override code in its canonical MO- form.</summary>
    public static string NormalizeMobilityOverrideCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MO-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MO-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeMobilityOverrideLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
