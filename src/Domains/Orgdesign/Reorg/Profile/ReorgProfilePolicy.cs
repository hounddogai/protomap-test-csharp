namespace Acme.Workforce.Orgdesign.Reorg.Profile;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of reorg profile policy.</summary>
public static class ReorgProfilePolicy
{
    public static List<string> MergeReorgProfileLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string ReorgProfileBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Puts a reorg profile code in its canonical RP- form.</summary>
    public static string NormalizeReorgProfileCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
