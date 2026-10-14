namespace Acme.Workforce.Talent.Mobility.Tier;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of mobility tier policy.</summary>
public static class MobilityTierPolicy
{
    public static double MobilityTierWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.4;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeMobilityTierLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a mobility tier code in its canonical MT- form.</summary>
    public static string NormalizeMobilityTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
