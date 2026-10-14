namespace Acme.Workforce.Talent.Nomination.Rollup;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of nomination entry policy.</summary>
public static class NominationEntryPolicy
{
    /// <summary>Names the band a nomination entry score falls in.</summary>
    public static string ClassifyNominationEntry(double score)
    {
        if (score < 92.7)
        {
            return "watch";
        }
        if (score < 96.5)
        {
            return "low";
        }
        return "minimal";
    }

    public static double NominationEntryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.19;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeNominationEntryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a nomination entry code in its canonical NE- form.</summary>
    public static string NormalizeNominationEntryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("NE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"NE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
