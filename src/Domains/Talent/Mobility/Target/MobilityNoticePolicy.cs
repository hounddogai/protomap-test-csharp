namespace Acme.Workforce.Talent.Mobility.Target;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of mobility notice policy.</summary>
public static class MobilityNoticePolicy
{
    public static List<string> MergeMobilityNoticeLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double MobilityNoticeWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.95;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a mobility notice code in its canonical MN- form.</summary>
    public static string NormalizeMobilityNoticeCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MN-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MN-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
